using System;
using System.Collections;
using System.Reflection;
using System.Text;

namespace unillm
{
    /// <summary>
    /// 构造一个类型的提示词
    /// </summary>
    public class UnillmTypePropmtBuilder : IUnillmPromptBuilder
    {
        public const string PropertyFormat = @"
I will describe the attributes in the Json file in the format of `name, type, description, option`.
The `name` represents the attribute name.
The `type` represents the attribute type.
The `description` represents the attribute description.
The `option` represents the example values of the attribute, described in the format of `[R][op1, op2, ...]`. When there is an `R`, it indicates that the value of this attribute can only be one of the given values. The `[]` indicates several example values.
        ";

        /// <summary>
        /// 要构造的类型
        /// </summary>
        public Type Type { get; set; }

        public UnillmTypePropmtBuilder(Type type)
        {
            Type = type;
        }

        public string Build()
        {
            var propmtBuilder = new StringBuilder();
            propmtBuilder.AppendLine(UnillmJsonHelper.ToJson(Activator.CreateInstance(Type)));

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

            BuildClass(Type);

            return propmtBuilder.ToString();
        }
    }

    /// <summary>
    /// 构造一个类型的提示词
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class UnillmTypePropmtBuilder<T> : UnillmTypePropmtBuilder where T : new()
    {
        public UnillmTypePropmtBuilder() : base(typeof(T))
        {

        }
    }
}
