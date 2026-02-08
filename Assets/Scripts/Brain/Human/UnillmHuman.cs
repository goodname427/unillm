using System.Collections.Generic;

namespace unillm
{
    public interface IUnillmHuman
    {
    }

    public abstract class UnillmHuman<TInput, TOutput, TBrain> : IUnillmHuman where TBrain : UnillmBrain<TInput, TOutput> where TInput : new() where TOutput : new()
    {
        private TBrain _brain;
        public TBrain Brain
        {
            get { return _brain; }
        }

        private readonly List<IUnillmSense> _unillmSenses;

        private readonly List<IUnillmBody> _unillmBodies;

        public UnillmHuman()
        {
            _unillmSenses = new List<IUnillmSense>();
            _unillmBodies = new List<IUnillmBody>();
        }

        public virtual void Init()
        {
            _brain = MakeBrain();
            if (_brain == null)
            {
                UnillmLogger.Error("Brain is null");
            }

            _brain.OnThinkCompleted += OnThinkCompleted;

            _unillmSenses.Clear();
            foreach (var sense in CollectSenses())
            {
                _unillmSenses.Add(sense);
                sense.OnSensed += OnSensed;
                sense.OnEquiped(this);
            }

            _unillmBodies.Clear();
            foreach (var body in CollectBodies())
            {
                _unillmBodies.Add(body);
            }
        }

        public virtual void Uninit()
        {
            _brain.OnThinkCompleted -= OnThinkCompleted;

            foreach (var sense in _unillmSenses)
            {
                sense.OnSensed -= OnSensed;
                sense.OnUnequiped(this);
            }
            _unillmSenses.Clear();

            foreach (var sense in _unillmBodies)
            {

            }
            _unillmSenses.Clear();
        }

        /// <summary>
        /// 收集大脑
        /// </summary>
        /// <returns></returns>
        protected abstract TBrain MakeBrain();

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
        protected abstract void OnThinkCompleted(UnillmBrain<TInput, TOutput> brain, UnillmOnBrainThinkCompletedEventArgs<TInput, TOutput> args);
    }
}
