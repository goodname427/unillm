using System;
using System.Collections;
using System.Reflection;
using System.Text;

namespace unillm
{
    /// <summary>
    /// 用于带输入输出结构提示词的构建
    /// 例如 我们的输入都是一个对应的json对象，并且我们要求输出的也是一个对应的json对象
    /// </summary>
    public class UnillmInOutPropmtBuilder<TInput, TOutput> : IUnillmPromptBuilder where TInput : new() where TOutput : new()
    {
        private const string s_propertyFormat = @"
对某一个属性以`name,type,descrption,option`的格式进行描述。
name表示该属性的名称。
type表示该属性的类型。
description表示该属性的描述。
option表示该属性的示例值，以`[R][op1, op2, ...]`的格式进行描述，当存在R时表示该属性的值只能为从给出的值，[]中表示若干个示例值
        ";

        /// <summary>
        /// 提示词背景，会原封不同的放置在提示词最开始部分
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

                    BuildBaseMember(member, memberType, prefix, $"Array[{elementType}]");

                    // 数组元素为非基础类型式时才对数组元素进行描述
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
我会严格按照以下json格式进行输入：
{inputPropmt}
请你严格按照以下json格式进行输出（除json对象外不要有任何多余的内容）:
{outputPropmt}
            ";
        }
    }
}
