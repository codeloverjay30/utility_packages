using System;
using System.Collections.Generic;

namespace ExceptionWrappers.Utilities
{
    /// <summary>
    /// 實作重試邏輯的泛型執行器 (有回傳值版本)。
    /// </summary>
    /// <typeparam name="T">操作的回傳類型。</typeparam>
    public class GenericRetryWrapper<T>
    {
        private const int DEFAULT_MAX_ATTEMPTS = 3;
        private const int DEFAULT_DELAY_MS = 500;

        // 核心執行參數
        public Func<T> TryAction { get; private set; }
        public Action FinallyAction { get; private set; }
        public Dictionary<Type , Action<Exception>> ExceptionHandlers { get; private set; }

        // 重試參數
        public int MaxAttempts { get; private set; }
        public int DelayMilliseconds { get; private set; }

        /// <summary>
        /// 建構函式：初始化重試參數和執行邏輯。
        /// </summary>
        public GenericRetryWrapper(
            Func<T> tryAction ,
            Dictionary<Type , Action<Exception>> exceptionHandlers = null ,
            Action finallyAction = null ,
            int maxAttempts = DEFAULT_MAX_ATTEMPTS ,
            int delayMilliseconds = DEFAULT_DELAY_MS
        )
        {
            this.TryAction = tryAction;
            this.FinallyAction = finallyAction;
            this.ExceptionHandlers = exceptionHandlers ?? new Dictionary<Type , Action<Exception>>();
            this.MaxAttempts = maxAttempts;
            this.DelayMilliseconds = delayMilliseconds;

            this.ValidateConstraints();
        }

        #region private method
        private void ValidateConstraints()
        {
            if(this.TryAction == null)
            {
                throw new ArgumentNullException(nameof(this.TryAction));
            }
            if(this.MaxAttempts <= 0)
            {
                throw new ArgumentException("最大嘗試次數必須大於 0。" , nameof(this.MaxAttempts));
            }
        }
        #endregion

        /// <summary>
        /// 執行帶有重試邏輯的操作並回傳 T 類型值。
        /// </summary>
        /// <param name="defaultReturnValue">當所有嘗試失敗時回傳的值。</param>
        /// <returns>操作成功的結果，或失敗時回傳 defaultReturnValue。</returns>
        public T Execute(T defaultReturnValue)
        {
            int currentAttempt = 0;
            bool success = false;
            T finalResult = defaultReturnValue;

            // 核心迴圈
            while(!success && currentAttempt < this.MaxAttempts)
            {
                currentAttempt++;

                // 創建並使用您提供的 GenericExceptionWrapper<T> 進行單次嘗試
                var singleAttemptWrapper = new GenericExceptionWrapper<T>(
                    tryAction: this.TryAction ,
                    finallyAction: this.FinallyAction ,
                    exceptionHandlers: this.ExceptionHandlers
                );

                try
                {
                    // 執行單次嘗試。如果成功，則返回結果；如果未處理異常，則重新拋出。
                    // 這裡傳入 defaultReturnValue，作為單次 Wrapper 失敗時的回傳值。
                    finalResult = singleAttemptWrapper.Execute(defaultReturnValue);

                    // 如果程式碼執行到這裡且沒有拋出異常，表示單次嘗試成功
                    success = true;
                }
                catch(Exception ex)
                {
                    // 捕獲被 wrapper re-throw 的異常 (表示執行失敗，需要重試)
                    Console.WriteLine($"[重試] 第 {currentAttempt}/{this.MaxAttempts} 次嘗試失敗。原因: {ex.Message}");

                    if(currentAttempt < this.MaxAttempts)
                    {
                        // 還有重試機會，執行延遲
                        int currentDelay = this.DelayMilliseconds * currentAttempt;
                        Console.WriteLine($"[重試] 等待 {currentDelay} 毫秒後重試...");
                        Thread.Sleep(currentDelay);
                    }
                    else
                    {
                        // 最後一次嘗試失敗。finalResult 仍保持為 defaultReturnValue (或最後一次成功的結果，但此處為失敗，故應是 defaultReturnValue)
                        Console.WriteLine("[FATAL] 所有重試次數已用盡，操作最終失敗。回傳預設值。");
                    }
                }
            }

            return finalResult; // 回傳最終的結果（成功或預設值）
        }
    }
}
