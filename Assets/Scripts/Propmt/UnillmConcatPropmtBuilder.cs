using System.Collections.Generic;
using System.Text;

namespace unillm
{
    /// <summary>
    /// 用于拼接提示词
    /// </summary>
    public class UnillmConcatPropmtBuilder : IUnillmPromptBuilder
    {
        public List<string> Propmts { get; }

        public UnillmConcatPropmtBuilder(params string[] propmts)
        {
            Propmts = new List<string>();
            foreach (var propmt in propmts)
            {
                Propmts.Add(propmt);
            }
        }

        public string Build()
        {
            var builder = new StringBuilder();
            foreach (var propmt in Propmts)
            {
                builder.AppendLine(propmt);
            }

            return builder.ToString();
        }
    }
}
