using System.Collections.Generic;

namespace unillm
{
    /// <summary>
    /// 能够长期记忆一些东西
    /// </summary>
    public interface IUnillmMemory
    {
        /// <summary>
        /// 该记忆关键词
        /// </summary>
        string Key { get; }

        /// <summary>
        /// 记忆内容
        /// </summary>
        string Content { get; }
    }

    /// <summary>
    /// 记忆搜索参数
    /// </summary>
    public class UnillmMemorySerachArgs
    {
        /// <summary>
        /// 搜索关键字
        /// </summary>
        public string SearchKey { get; set; }
        /// <summary>
        /// 模糊搜索
        /// </summary>
        public bool FuzzySearch { get; set; }
    }

    /// <summary>
    /// 记忆搜索参数
    /// </summary>
    public class UnillmMemoryStoreArgs
    {
        public IUnillmMemory Memory { get; set; }
    }

    /// <summary>
    /// 记忆容器，用于存储记忆，查找记忆
    /// </summary>
    public interface IUnillmMemoryContainer : IEnumerable<IUnillmMemory>, IUnillmHumanEquipable
    {
        /// <summary>
        /// 搜索记忆
        /// </summary>
        /// <returns></returns>
        public IUnillmMemory Serach(UnillmMemorySerachArgs serachArgs);

        public bool Store();
    }
}
