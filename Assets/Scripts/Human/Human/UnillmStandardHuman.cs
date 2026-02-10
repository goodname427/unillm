using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;

namespace unillm
{
    public class UnillmStandardHumanInput
    {
        [UnillmPropmtDescription("The target you should got")]
        public string Target;

        [UnillmPropmtDescription("The sensed information")]
        public List<string> Sensed = new();
    }

    public class UnillmStandardHumanAction
    {
        [UnillmPropmtDescription("Action Name")]
        public string Name = "example name";

        [UnillmPropmtDescription("Action Args, please using a json object fill it")]
        public string Args = "{}";

        public T Get<T>(Type argsType = null) where T : new()
        {
            return UnillmJsonHelper.ToObject<T>(UnillmJsonHelper.GetCleanJson(Args), argsType);
        }
    }

    public class UnillmStandardHumanOutput
    {
        [UnillmPropmtDescription("The actions you wanna do, it will be execute by order")]
        public UnillmStandardHumanAction[] Actions = { new() };
    }

    public class UnillmStandardHumanStartTurnArgs
    {
        public string Target { get; set; }

        public UnillmStandardHumanInput OverrideInput { get; set; }
    }

    public enum UnillmStandardHumanActionExecuteFailedReason
    {
        /// <summary>
        /// 表示成功
        /// </summary>
        None,

        /// <summary>
        /// Body没有找到
        /// </summary>
        BodyNotFound,

        /// <summary>
        /// Body执行失败
        /// </summary>
        BodyDoFailed,
    }

    public class UnillmStandardHumanActionExecuteResult
    {
        public string Name { get; set; }
        public UnillmBodyDoEventArgs Args { get; set; }

        public T Get<T>() where T : UnillmBodyDoEventArgs
        {
            return Args as T;
        }

        public UnillmStandardHumanActionExecuteFailedReason FailedReason = UnillmStandardHumanActionExecuteFailedReason.None;
        public UnillmBodyDoResult DoResult = new();

        public bool IsSuccess => FailedReason == UnillmStandardHumanActionExecuteFailedReason.None && DoResult.IsSuccess;
    }

    public class UnillmOnStandardHumanTurnCompletedEventArgs : UnillmFuctionalEventArgs
    {
        public UnillmStandardHumanInput Input { get; set; }
        public UnillmStandardHumanOutput Output { get; set; }

        public List<UnillmStandardHumanActionExecuteResult> ActionExecuteResults { get; set; } = new();

        public bool IsAllActionExecuteSuccess => IsSuccess && ActionExecuteResults.All(result => result.IsSuccess);
        public bool IsAnyActionExecuteSuccess => IsSuccess && ActionExecuteResults.Any(result => result.IsSuccess);
    }

    public delegate void UnillmOnStandardHumanTurnCompletedEventHandler(
        UnillmStandardHuman human,
        UnillmOnStandardHumanTurnCompletedEventArgs args);

    /// <summary>
    /// 标准Human实现，自动理解所携带的能力并使用。
    /// 单轮对话方式，即每回合只与Agent对话一次。
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    public abstract class UnillmStandardHuman : UnillmCommonHuman<UnillmStandardHumanInput, UnillmStandardHumanOutput>
    {
        /// <summary>
        /// 回合结束时调用
        /// </summary>
        public event UnillmOnStandardHumanTurnCompletedEventHandler OnTurnCompleted;

        /// <summary>
        /// 累计的输入
        /// </summary>
        protected UnillmStandardHumanInput CachedInput { get; private set; } = new();

        /// <summary>
        /// 大脑正在思考说明正在回合中
        /// </summary>
        public bool IsOnTurn => Brain.IsThinking;

        /// <summary>
        /// 获取该Human的背景介绍
        /// </summary>
        /// <returns></returns>
        public abstract string MakeBackground();

        /// <summary>
        /// 创建Agent
        /// </summary>
        /// <returns></returns>
        protected virtual IUnillmAgent MakeAgent()
        {
            return new UnillmCommmonAgent();
        }

        protected override IUnillmBrain<UnillmStandardHumanInput, UnillmStandardHumanOutput> MakeBrain()
        {
            var prompt = new UnillmStandardHumanPromptBuilder(this).Build();
            var brain = new UnillmCommonBrain<UnillmStandardHumanInput, UnillmStandardHumanOutput>(new UnillmCommonBrainInitConfig(
                MakeAgent(), 
                prompt, 
                false)
            );
            return brain;
        }

        protected override void OnSensed(IUnillmSense sense, UnillmOnSensedEventArgs args)
        {
            CachedInput ??= new UnillmStandardHumanInput();

            CachedInput.Sensed.Add(args.Sensed);
        }

        protected override void OnThinkCompleted(UnillmCommonBrain<UnillmStandardHumanInput, UnillmStandardHumanOutput> brain, UnillmOnBrainThinkCompletedEventArgs<UnillmStandardHumanInput, UnillmStandardHumanOutput> args)
        {
            var onTurnCompletedArgs = new UnillmOnStandardHumanTurnCompletedEventArgs()
            {
                ErrorReason = args.ErrorReason,
                Input = args.Input,
                Output = args.Output
            };

            if (!args.IsSuccess)
            {
                OnTurnCompleted?.Invoke(this, onTurnCompletedArgs);
                return;
            }

            // 子类预留接口
            if (!CheckArgs(args, out var reason))
            {
                onTurnCompletedArgs.ErrorReason = reason;
                OnTurnCompleted?.Invoke(this, onTurnCompletedArgs);
                return;
            }

            foreach (var action in args.Output.Actions)
            {
                var executeResult = new UnillmStandardHumanActionExecuteResult
                {
                    Name = action.Name
                };
                onTurnCompletedArgs.ActionExecuteResults.Add(executeResult);

                var body = Bodies.FirstOrDefault(body => body.Name == action.Name);
                if (body == null)
                {
                    UnillmLogger.Warrning($"Human wanna do a not exsist action {action.Name}");
                    executeResult.FailedReason = UnillmStandardHumanActionExecuteFailedReason.BodyNotFound;
                    continue;
                }

                executeResult.Args = action.Get<UnillmBodyDoEventArgs>(body.ArgsType);

                if (!body.Do(executeResult.Args, executeResult.DoResult))
                {
                    executeResult.FailedReason = UnillmStandardHumanActionExecuteFailedReason.BodyDoFailed;
                }
            }

            OnTurnCompleted?.Invoke(this, onTurnCompletedArgs);
        }

        protected bool CheckArgs(UnillmOnBrainThinkCompletedEventArgs<UnillmStandardHumanInput, UnillmStandardHumanOutput> args, out string reason)
        {
            reason = null;
            return true;
        }

        /// <summary>
        /// 开始回合
        /// </summary>
        public virtual bool StartTurn(UnillmStandardHumanStartTurnArgs startTurnArgs = null)
        {
            if (IsOnTurn)
            {
                UnillmLogger.Warrning("Still on turn");
                return false;
            }

            startTurnArgs ??= new UnillmStandardHumanStartTurnArgs();

            // 设置输入参数
            CachedInput = startTurnArgs.OverrideInput ?? CachedInput;
            CachedInput ??= new UnillmStandardHumanInput();
            CachedInput.Target = startTurnArgs.Target;

            if (Brain.Think(CachedInput))
            {
                CachedInput = null;
                return true;
            }

            return false;
        }
    }
}
