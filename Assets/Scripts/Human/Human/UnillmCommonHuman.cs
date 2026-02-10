using System.Collections.Generic;
using UnityEngine.AI;

namespace unillm
{
    /// <summary>
    /// 通用的Human实现
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    public abstract class UnillmCommonHuman<TInput, TOutput> : IUnillmHuman<TInput, TOutput> where TInput : new() where TOutput : new()
    {
        private IUnillmBrain<TInput, TOutput> _brain;
        public IUnillmBrain<TInput, TOutput> Brain => _brain;

        private readonly List<IUnillmSense> _senses;
        public IReadOnlyList<IUnillmSense> Senses => _senses;

        private readonly List<IUnillmBody> _bodies;
        public IReadOnlyList<IUnillmBody> Bodies => _bodies; 

        public bool HasInit { get; private set; }

        public UnillmCommonHuman()
        {
            _senses = new List<IUnillmSense>();
            _bodies = new List<IUnillmBody>();
        }

        /// <summary>
        /// 初始化Human的各个部件
        /// </summary>
        public virtual void Init()
        {
            if (HasInit)
            {
                UnillmLogger.Error("Human has init");
                return;
            }
            HasInit = true;

            _senses.Clear();
            foreach (var sense in CollectSenses())
            {
                _senses.Add(sense);
                sense.OnSensed += OnSensed;
                sense.OnEquipped(this);
            }

            _bodies.Clear();
            foreach (var body in CollectBodies())
            {
                _bodies.Add(body);
                body.OnEquipped(this);
            }

            _brain = MakeBrain();
            if (_brain == null)
            {
                UnillmLogger.Error("Brain is null");
            }

            _brain.OnEquipped(this);
            _brain.OnThinkCompleted += OnThinkCompleted;
        }

        /// <summary>
        /// 注销
        /// </summary>
        public virtual void Uninit()
        {
            if (!HasInit)
            {
                UnillmLogger.Error("Human has not init");
                return;
            }

            _brain.OnUnequipped(this);
            _brain.OnThinkCompleted -= OnThinkCompleted;

            foreach (var sense in _senses)
            {
                sense.OnSensed -= OnSensed;
                sense.OnUnequipped(this);
            }
            _senses.Clear();

            foreach (var body in _bodies)
            {
                body.OnUnequipped(this);
            }
            _senses.Clear();

            HasInit = false;
        }

        /// <summary>
        /// 收集大脑（必须初始化完毕）
        /// </summary>
        /// <returns></returns>
        protected abstract IUnillmBrain<TInput, TOutput> MakeBrain();

        /// <summary>
        /// 收集该Human所拥有的所有Sense
        /// </summary>
        /// <returns></returns>
        protected abstract IEnumerable<IUnillmSense> CollectSenses();

        /// <summary>
        /// 收起该Human所拥有的所有Body
        /// </summary>
        /// <returns></returns>
        protected abstract IEnumerable<IUnillmBody> CollectBodies();

        /// <summary>
        /// 接受到任意输入时
        /// </summary>
        /// <param name="sense"></param>
        /// <param name="args"></param>
        protected abstract void OnSensed(IUnillmSense sense, UnillmOnSensedEventArgs args);

        /// <summary>
        /// 当思考结束时
        /// </summary>
        /// <param name="brain"></param>
        /// <param name="args"></param>
        protected abstract void OnThinkCompleted(UnillmCommonBrain<TInput, TOutput> brain, UnillmOnBrainThinkCompletedEventArgs<TInput, TOutput> args);
    }
}