using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Windows;

namespace unillm
{
    public class UnillmStandardHumanPromptBuilder : IUnillmPromptBuilder
    {
        /// <summary>
        /// 提示词背景，会原封不同的放置在提示词最开始部分
        /// </summary>
        public UnillmStandardHuman Human { get; set; }

        public UnillmStandardHumanPromptBuilder(UnillmStandardHuman human)
        {
            Human = human;
        }

        public string Build()
        {
            if (Human == null)
            {
                UnillmLogger.Error("Using a null human to build propmt");
                return null;
            }    

            var bodyPropmt = new StringBuilder();

            foreach (var body in Human.Bodies)
            {
                bodyPropmt.AppendLine(new UnillmBodyPropmtBuilder(body).Build());
            }

            var inOutPropmt = new UnillmInOutPropmtBuilder<UnillmStandardHumanInput, UnillmStandardHumanOutput>().Build();
            
            return $@"
{Human.MakeBackground()}
{UnillmTypePropmtBuilder.PropertyFormat}
You can perform the following actions：
{bodyPropmt}
{inOutPropmt}
            ";

        }
    }
}