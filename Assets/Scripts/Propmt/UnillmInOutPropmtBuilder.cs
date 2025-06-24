using System;
using System.Collections;
using System.Reflection;
using System.Text;

namespace unillm
{
    /// <summary>
    /// ���ڴ���������ṹ��ʾ�ʵĹ���
    /// ���� ���ǵ����붼��һ����Ӧ��json���󣬲�������Ҫ�������Ҳ��һ����Ӧ��json����
    /// </summary>
    public class UnillmInOutPropmtBuilder<TInput, TOutput> : IUnillmPromptBuilder where TInput : new() where TOutput : new()
    {
        private const string s_propertyFormat = @"
��ĳһ��������`name,type,descrption,option`�ĸ�ʽ����������
name��ʾ�����Ե����ơ�
type��ʾ�����Ե����͡�
description��ʾ�����Ե�������
option��ʾ�����Ե�ʾ��ֵ����`[R][op1, op2, ...]`�ĸ�ʽ����������������Rʱ��ʾ�����Ե�ֵֻ��Ϊ�Ӹ�����ֵ��[]�б�ʾ���ɸ�ʾ��ֵ
        ";

        /// <summary>
        /// ��ʾ�ʱ�������ԭ�ⲻͬ�ķ�������ʾ���ʼ����
        /// </summary>
        public string Background { get; set; }

        public UnillmInOutPropmtBuilder(string background = "")
        {
            Background = background;
        }

        private string Build<T>() where T : new()
        {
            var propmtBuilder = new StringBuilder();
            propmtBuilder.AppendLine(UnillmJsonHelper.ToJson(new T()));

            void BuildBaseMember(MemberInfo member, Type memberType, string prefix = "", string overrideType = "")
            {
                var descriptionPropmt = "";
                var descriptionAttr = member.GetCustomAttribute<UnillmPropmtDescriptionAttribute>();
                if (descriptionAttr is not null)
                {
                    descriptionPropmt = descriptionAttr.Description;
                }

                string optionPropmt = "";
                var optionsAttr = member.GetCustomAttribute<UnillmPropmtOptionAttribute>();
                if (optionsAttr is not null || memberType.IsEnum)
                {
                    bool restrict;
                    string[] options;
                    if (memberType.IsEnum)
                    {
                        restrict = true;
                        options = Enum.GetNames(memberType);
                    }
                    else
                    {
                        restrict = optionsAttr.Restrict;
                        options = optionsAttr.Options;
                    }

                    optionPropmt = $"{(restrict ? "R" : "")}[{string.Join(",", options)}]";
                }

                propmtBuilder.AppendLine($"{prefix}{member.Name},{(string.IsNullOrEmpty(overrideType) ? memberType.Name : overrideType)},{descriptionPropmt},{optionPropmt}");
            }

            void BuildMember(MemberInfo member, Type memberType, string prefix = "")
            {
                if (memberType == typeof(bool))
                {
                    BuildBaseMember(member, memberType, prefix);
                    return;
                }

                if (memberType == typeof(float))
                {
                    BuildBaseMember(member, memberType, prefix);
                    return;
                }

                if (memberType == typeof(double))
                {
                    BuildBaseMember(member, memberType, prefix);
                    return;
                }

                if (memberType == typeof(int))
                {
                    BuildBaseMember(member, memberType, prefix);
                    return;
                }

                if (memberType == typeof(long))
                {
                    BuildBaseMember(member, memberType, prefix);
                    return;
                }

                if (memberType == typeof(string))
                {
                    BuildBaseMember(member, memberType, prefix);
                    return;
                }

                if (typeof(Enum).IsAssignableFrom(memberType))
                {
                    BuildBaseMember(member, memberType, prefix, "String");
                    return;
                }

                if (typeof(IList).IsAssignableFrom(memberType))
                {
                    BuildBaseMember(member, memberType, prefix, "Array");

                    Type elementType;
                    if (memberType.IsArray)
                    {
                        elementType = memberType.GetElementType();
                    }
                    else if (memberType.IsGenericType)
                    {
                        elementType = memberType.GetGenericArguments()[0];
                    }
                    else
                    {
                        return;
                    }

                    // ����Ԫ��Ϊ�ǻ�������ʽʱ�Ŷ�����Ԫ�ؽ�������
                    if (elementType != typeof(bool) 
                        && elementType != typeof(float)
                        && elementType != typeof(double)
                        && elementType != typeof(int)
                        && elementType != typeof(long)
                        && elementType != typeof(string)
                        && elementType != typeof(Enum)
                        && elementType != typeof(IList))
                    {
                        BuildClass(elementType, $"{prefix}{member.Name}.element.");
                    }

                    return;
                }

                BuildClass(memberType, $"{prefix}{member.Name}.");
            }

            void BuildClass(Type classType, string prefix = "")
            {
                foreach (var field in classType.GetFields())
                {
                    BuildMember(field, field.FieldType, prefix);
                }

                foreach (var property in classType.GetProperties())
                {
                    BuildMember(property, property.PropertyType, prefix);
                }
            }

            BuildClass(typeof(T));

            return propmtBuilder.ToString();
        }

        public string Build()
        {
            var inputPropmt = Build<TInput>();
            var outputPropmt = Build<TOutput>();

            return $@"
{Background}
{s_propertyFormat}
�һ��ϸ�������json��ʽ�������룺
{inputPropmt}
�����ϸ�������json��ʽ�����������json�����ⲻҪ���κζ�������ݣ�:
{outputPropmt}
            ";
        }
    }
}
