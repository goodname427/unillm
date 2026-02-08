using System.Collections.Generic;

namespace unillm
{
    public abstract class UnillmHuman<TInput, TOutput> : IUnillmHuman<TInput, TOutput> where TInput : new() where TOutput : new()
    {
        private IUnillmBrain<TInput, TOutput> _brain;
        public IUnillmBrain<TInput, TOutput> Brain
        {
            get { return _brain; }
        }

        private readonly List<IUnillmSense> _senses;
        public IReadOnlyList<IUnillmSense> Senses => _senses;

        private readonly List<IUnillmBody> _bodies;
        public IReadOnlyList<IUnillmBody> Bodies => _bodies; 

        public UnillmHuman()
        {
            _senses = new List<IUnillmSense>();
            _bodies = new List<IUnillmBody>();
        }

        public virtual void Init()
        {
            _brain = MakeBrain();
            if (_brain == null)
            {
                UnillmLogger.Error("Brain is null");
            }

            _brain.OnThinkCompleted += OnThinkCompleted;

            _senses.Clear();
            foreach (var sense in CollectSenses())
            {
                _senses.Add(sense);
                sense.OnSensed += OnSensed;
                sense.OnEquiped(this);
            }

            _bodies.Clear();
            foreach (var body in CollectBodies())
            {
                _bodies.Add(body);
            }
        }

        public virtual void Uninit()
        {
            _brain.OnThinkCompleted -= OnThinkCompleted;

            foreach (var sense in _senses)
            {
                sense.OnSensed -= OnSensed;
                sense.OnUnequiped(this);
            }
            _senses.Clear();

            foreach (var sense in _bodies)
            {

            }
            _senses.Clear();
        }

        /// <summary>
        /// 收集大脑
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