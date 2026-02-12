using System.Collections.Generic;

namespace unillm
{
    /// <summary>
    /// 可以被Human装备的对象
    /// </summary>
    public interface IUnillmHumanEquipable
    {
        void OnEquipped<TInput, TOutput>(IUnillmHuman<TInput, TOutput> human) where TInput : new() where TOutput : new() { }
        void OnUnequipped<TInput, TOutput>(IUnillmHuman<TInput, TOutput> human) where TInput : new() where TOutput : new() { }
    }

    /// <summary>
    /// 可以被Human装备的对象
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    public interface IUnillmHumanEquipable<TInput, TOutput> where TInput : new() where TOutput : new()
    {
        void OnEquipped(IUnillmHuman<TInput, TOutput> human);
        void OnUnequipped(IUnillmHuman<TInput, TOutput> human);
    }    

    /// <summary>
    /// 具备感知世界，思考，并做出行动等能力的集合体
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    public interface IUnillmHuman<TInput, TOutput> where TInput : new() where TOutput : new()
    {
        /// <summary>
        /// 大脑
        /// </summary>
        IUnillmBrain<TInput, TOutput> Brain { get; }
        
        /// <summary>
        /// 感知
        /// </summary>
        IReadOnlyList<IUnillmSense> Senses { get; }
        
        /// <summary>
        /// 身体
        /// </summary>
        IReadOnlyList<IUnillmBody> Bodies { get; }
    }
}
