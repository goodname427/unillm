using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using UnityEngine.Experimental.GlobalIllumination;

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

    /// <summary>
    /// 回合开始参数
    /// </summary>
    public class UnillmStandardHumanStartTurnArgs
    {
        public string Target { get; set; }

        public UnillmStandardHumanInput OverrideInput { get; set; }
    }

    /// <summary>
    /// Action执行失败原因
    /// </summary>
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

    /// <summary>
    /// Action执行结果
    /// </summary>
    public class UnillmStandardHumanActionExecuteResult
    {
        public string Name { get; set; }
        public UnillmBodyDoEventArgs Args { get; set; }

        public T Get<T>() where T : UnillmBodyDoEventArgs
        {
            return Args as T;
        }

        /// <summary>
        /// 失败类型
        /// </summary>
        public UnillmStandardHumanActionExecuteFailedReason FailedReasonType = UnillmStandardHumanActionExecuteFailedReason.None;
        /// <summary>
        /// 具体执行结果
        /// </summary>
        public UnillmBodyDoResult DoResult = new();

        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuccess => FailedReasonType == UnillmStandardHumanActionExecuteFailedReason.None && DoResult.IsSuccess;
        
        /// <summary>
        /// 失败理由
        /// </summary>
        public string FailedReson => FailedReasonType switch
        {
            UnillmStandardHumanActionExecuteFailedReason.None => null,
            UnillmStandardHumanActionExecuteFailedReason.BodyNotFound => $"Human wanna do a not exsist action {Name}",
            UnillmStandardHumanActionExecuteFailedReason.BodyDoFailed => DoResult.ErrorReason,
            _ => null,
        };
    }

    /// <summary>
    /// 回合结束
    /// 注意，回合结束成功与否与Action的执行情况无关
    /// </summary>
    public class UnillmOnStandardHumanTurnCompletedEventArgs : UnillmFuctionalEventArgs
    {
        /// <summary>
        /// 当前回合输入
        /// </summary>
        public UnillmStandardHumanInput Input { get; set; }
        
        /// <summary>
        /// 当前回合输出
        /// </summary>
        public UnillmStandardHumanOutput Output { get; set; }

        /// <summary>
        /// Action执行情况
        /// </summary>
        public List<UnillmStandardHumanActionExecuteResult> ActionExecuteResults { get; set; } = new();

        /// <summary>
        /// 是否所有Action都执行成功
        /// </summary>
        public bool IsAllActionExecuteSuccess => IsSuccess && ActionExecuteResults.All(result => result.IsSuccess);
        
        /// <summary>
        /// 是否所有Action都执行失败
        /// </summary>
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
                    executeResult.FailedReasonType = UnillmStandardHumanActionExecuteFailedReason.BodyNotFound;
                    continue;
                }

                executeResult.Args = action.Get<UnillmBodyDoEventArgs>(body.ArgsType);

                if (!body.Do(executeResult.Args, executeResult.DoResult))
                {
                    executeResult.FailedReasonType = UnillmStandardHumanActionExecuteFailedReason.BodyDoFailed;
                }
            }

            OnTurnCompleted?.Invoke(this, onTurnCompletedArgs);
        }

        protected virtual bool CheckArgs(UnillmOnBrainThinkCompletedEventArgs<UnillmStandardHumanInput, UnillmStandardHumanOutput> args, out string reason)
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
