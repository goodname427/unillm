using System.Collections.Generic;

namespace unillm
{
    public interface IUnillmEquipable
    {
        void OnEquipped<TInput, TOutput>(IUnillmHuman<TInput, TOutput> human) where TInput : new() where TOutput : new() { }
        void OnUnequipped<TInput, TOutput>(IUnillmHuman<TInput, TOutput> human) where TInput : new() where TOutput : new() { }
    }

    public interface IUnillmEquipable<TInput, TOutput> where TInput : new() where TOutput : new()
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
