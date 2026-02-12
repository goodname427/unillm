using System;
using System.Collections.Generic;
using System.Linq;

namespace unillm
{
    /// <summary>
    /// 标准输入
    /// </summary>
    public class UnillmBodyHumanInput
    {
        [UnillmPropmtDescription("The target you should got")]
        public string Target;

        [UnillmPropmtDescription("The sensed information")]
        public List<string> Sensed = new();
    }

    /// <summary>
    /// 标准动作
    /// </summary>
    public class UnillmBodyHumanAction
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

    /// <summary>
    /// 标准输出
    /// </summary>
    public class UnillmBodyHumanOutput
    {
        [UnillmPropmtDescription("The actions you wanna do, it will be execute by order")]
        public UnillmBodyHumanAction[] Actions = { new() };
    }

    /// <summary>
    /// 回合开始参数
    /// </summary>
    public class UnillmBodyHumanStartTurnArgs
    {
        public string Target { get; set; }

        public UnillmBodyHumanInput OverrideInput { get; set; }
    }

    /// <summary>
    /// Action执行失败原因
    /// </summary>
    public enum UnillmBodyHumanActionExecuteErrorReason
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
        /// 参数转换失败
        /// </summary>
        ArgsConvertFailed,

        /// <summary>
        /// Body执行失败
        /// </summary>
        BodyDoFailed,
    }

    /// <summary>
    /// Action执行结果
    /// </summary>
    public class UnillmBodyHumanActionExecuteResult : UnillmFuctionalEventArgs
    {
        public string Name { get; set; }
        public UnillmBodyDoArgs Args { get; set; }
        public IUnillmBody Body { get; set; }

        public T Get<T>() where T : UnillmBodyDoArgs
        {
            return Args as T;
        }

        private UnillmBodyHumanActionExecuteErrorReason _errorReasonType = UnillmBodyHumanActionExecuteErrorReason.None;
        /// <summary>
        /// 失败类型
        /// </summary>
        public UnillmBodyHumanActionExecuteErrorReason ErrorReasonType
        {
            get => _errorReasonType;
            set
            {
                if (_errorReasonType != UnillmBodyHumanActionExecuteErrorReason.None && _errorReasonType != UnillmBodyHumanActionExecuteErrorReason.BodyDoFailed)
                {
                    UnillmLogger.Error(ErrorReason);
                }
                _errorReasonType = value;
            }
        }

        /// <summary>
        /// 具体执行结果
        /// </summary>
        public UnillmBodyDoResult DoResult { get; set; } = new();

        public override bool IsSuccess => ErrorReasonType == UnillmBodyHumanActionExecuteErrorReason.None && DoResult.IsSuccess;

        public override string ErrorReason => ErrorReasonType switch
        {
            UnillmBodyHumanActionExecuteErrorReason.None => null,
            UnillmBodyHumanActionExecuteErrorReason.BodyNotFound => $"Human wanna do a not exsist action {Name}",
            UnillmBodyHumanActionExecuteErrorReason.ArgsConvertFailed => $"Cant convert args to type({Body.ArgsType})，please ",
            UnillmBodyHumanActionExecuteErrorReason.BodyDoFailed => DoResult.ErrorReason,
            _ => null,
        };
    }

    /// <summary>
    /// 回合执行结果
    /// 注意，回合结束成功与否与Action的执行情况无关
    /// </summary>
    public class UnillmOnBodyHumanTurnCompletedEventArgs : UnillmFuctionalEventArgs
    {
        /// <summary>
        /// 当前回合输入
        /// </summary>
        public UnillmBodyHumanInput Input { get; set; }

        /// <summary>
        /// 当前回合输出
        /// </summary>
        public UnillmBodyHumanOutput Output { get; set; }

        /// <summary>
        /// Action执行情况
        /// </summary>
        public List<UnillmBodyHumanActionExecuteResult> ActionExecuteResults { get; set; } = new();

        /// <summary>
        /// 是否所有Action都执行成功
        /// </summary>
        public bool IsAllActionExecuteSuccess => IsSuccess && ActionExecuteResults.All(result => result.IsSuccess);

        /// <summary>
        /// 是否所有Action都执行失败
        /// </summary>
        public bool IsAnyActionExecuteSuccess => IsSuccess && ActionExecuteResults.Any(result => result.IsSuccess);
    }

    public delegate void UnillmOnBodyHumanTurnCompletedEventHandler(
        UnillmBodyHuman human,
        UnillmOnBodyHumanTurnCompletedEventArgs args);

    /// <summary>
    /// 自动理解所携带的Body并使用。
    /// 单轮对话方式，即每回合只与Agent对话一次。
    /// Gameplay => Sense => Brain Input => Brain => Brain Out => Body => Gameplay
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    public abstract class UnillmBodyHuman : UnillmCommonHuman<UnillmBodyHumanInput, UnillmBodyHumanOutput>
    {
        /// <summary>
        /// 回合结束时调用
        /// </summary>
        public event UnillmOnBodyHumanTurnCompletedEventHandler OnTurnCompleted;

        /// <summary>
        /// 累计的输入
        /// </summary>
        protected UnillmBodyHumanInput CachedInput { get; private set; } = new();

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

        protected override IUnillmBrain<UnillmBodyHumanInput, UnillmBodyHumanOutput> MakeBrain()
        {
            var prompt = new UnillmStandardHumanPromptBuilder(this).Build();
            var brain = new UnillmCommonBrain<UnillmBodyHumanInput, UnillmBodyHumanOutput>(new UnillmCommonBrainInitConfig(
                MakeAgent(),
                prompt,
                false)
            );
            return brain;
        }

        protected override void OnSensed(IUnillmSense sense, UnillmOnSensedEventArgs args)
        {
            CachedInput ??= new UnillmBodyHumanInput();

            CachedInput.Sensed.Add(args.Sensed);
        }

        protected override void OnThinkCompleted(UnillmCommonBrain<UnillmBodyHumanInput, UnillmBodyHumanOutput> brain, UnillmOnBrainThinkCompletedEventArgs<UnillmBodyHumanInput, UnillmBodyHumanOutput> args)
        {
            var onTurnCompletedArgs = new UnillmOnBodyHumanTurnCompletedEventArgs()
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
            if (!CheckBrainThinkArgs(args, out var reason))
            {
                onTurnCompletedArgs.ErrorReason = reason;
                OnTurnCompleted?.Invoke(this, onTurnCompletedArgs);
                return;
            }

            foreach (var action in args.Output.Actions)
            {
                var executeResult = new UnillmBodyHumanActionExecuteResult
                {
                    Name = action.Name
                };
                onTurnCompletedArgs.ActionExecuteResults.Add(executeResult);

                executeResult.Body = Bodies.FirstOrDefault(body => body.Name == action.Name);
                if (executeResult.Body == null)
                {
                    executeResult.ErrorReasonType = UnillmBodyHumanActionExecuteErrorReason.BodyNotFound;
                    continue;
                }

                executeResult.Args = action.Get<UnillmBodyDoArgs>(executeResult.Body.ArgsType);
                if (executeResult.Args == null)
                {
                    executeResult.ErrorReasonType = UnillmBodyHumanActionExecuteErrorReason.ArgsConvertFailed;
                    continue;
                }

                if (!executeResult.Body.Do(executeResult.Args, executeResult.DoResult))
                {
                    executeResult.ErrorReasonType = UnillmBodyHumanActionExecuteErrorReason.BodyDoFailed;
                }
            }

            OnTurnCompleted?.Invoke(this, onTurnCompletedArgs);
        }

        /// <summary>
        /// 检查Brain思考的输入
        /// </summary>
        /// <param name="args"></param>
        /// <param name="reason"></param>
        /// <returns></returns>
        protected virtual bool CheckBrainThinkArgs(UnillmOnBrainThinkCompletedEventArgs<UnillmBodyHumanInput, UnillmBodyHumanOutput> args, out string reason)
        {
            reason = null;
            return true;
        }

        /// <summary>
        /// 开始回合
        /// </summary>
        public virtual bool StartTurn(UnillmBodyHumanStartTurnArgs startTurnArgs = null)
        {
            if (IsOnTurn)
            {
                UnillmLogger.Warrning("Still on turn");
                return false;
            }

            startTurnArgs ??= new UnillmBodyHumanStartTurnArgs();

            // 设置输入参数
            CachedInput = startTurnArgs.OverrideInput ?? CachedInput;
            CachedInput ??= new UnillmBodyHumanInput();
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
