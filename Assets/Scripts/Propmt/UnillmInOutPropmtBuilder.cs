using System;

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
        public string Build()
        {
            var inputPropmt = new UnillmTypePropmtBuilder<TInput>().Build();
            var outputPropmt = new UnillmTypePropmtBuilder<TOutput>().Build();

            return $@"
I will input strictly in accordance with the following JSON format: 
{inputPropmt}
Please output strictly in the following JSON format (no additional content except for the JSON object). 
{outputPropmt}
            ";
        }
    }
}
