using System;

namespace unillm
{
    /// <summary>
    /// ����һ���ֶλ�������
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
    /// �ṩһ���ֶλ������Ե�ʾ��ֵ�������ж��
    /// ͬʱ�������޶�ֻ�ܷ��ظ�����ֵ
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
