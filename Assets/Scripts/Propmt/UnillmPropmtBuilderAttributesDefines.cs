using System;

namespace unillm
{
    /// <summary>
    /// 描述一个字段或者属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class UnillmPropmtDescriptionAttribute : Attribute
    {
        public string Description { get; }

        public UnillmPropmtDescriptionAttribute(string description)
        {
            Description = description;
        }
    }

    /// <summary>
    /// 提供一个字段或者属性的示例值，可以有多个
    /// 同时，可以限定只能返回给定的值
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class UnillmPropmtOptionAttribute : Attribute
    {
        public string[] Options { get; }
        public bool Restrict { get; }

        public UnillmPropmtOptionAttribute(bool restrict ,params string[] options)
        {
            Restrict = restrict;
            Options = options;

            if (Restrict && Options.Length < 2)
            {
                UnillmLogger.Warrning($"The Feild is restrict but offered example count({Options.Length}) less than 2");
            }
        }

        public UnillmPropmtOptionAttribute(params string[] example) : this(false, example)
        {
        }
    }

}
