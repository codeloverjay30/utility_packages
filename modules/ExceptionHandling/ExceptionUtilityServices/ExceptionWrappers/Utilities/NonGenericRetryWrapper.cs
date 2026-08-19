using System;
using System.Collections.Generic;

namespace ExceptionWrappers.Utilities
{
    /// <summary>
    /// 實作重試邏輯的非靜態執行器 (無回傳值版本)。
    /// </summary>
    public class NonGenericRetryWrapper
    {
        private const int DEFAULT_MAX_ATTEMPTS = 3;
        private const int DEFAULT_DELAY_MS = 500;

        // 核心邏輯
        public Action TryAction { get; private set; }
        public Action FinallyAction { get; private set; }
        public Dictionary<Type , Action<Exception>> ExceptionHandlers { get; private set; }

        // 重試參數
        public int MaxAttempts { get; private set; }
        public int DelayMilliseconds { get; private set; }

        /// <summary>
        /// 建構函式：初始化重試參數和執行邏輯。
        /// </summary>
        public NonGenericRetryWrapper(
            Action tryAction ,
            Dictionary<Type , Action<Exception>> exceptionHandlers = null ,
            Action finallyAction = null ,
            int maxAttempts = DEFAULT_MAX_ATTEMPTS ,
            int delayMilliseconds = DEFAULT_DELAY_MS)
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
        /// 執行帶有重試邏輯的操作。
        /// </summary>
        /// <returns>如果操作最終成功則回傳 true；否則回傳 false。</returns>
        public bool Execute()
        {
            int currentAttempt = 0;
            bool success = false;

            // 核心迴圈：只要還沒成功 AND 嘗試次數未達上限，就繼續執行
            while(!success && currentAttempt < this.MaxAttempts)
            {
                currentAttempt++;

                // 在迴圈內創建 NonGenericExceptionWrapper，以執行單次嘗試
                var singleAttemptWrapper = new NonGenericExceptionWrapper(
                    tryAction: this.TryAction ,
                    finallyAction: this.FinallyAction ,
                    exceptionHandlers: this.ExceptionHandlers
                );

                try
                {
                    // 執行單次嘗試。如果發生未處理的異常，它將會被重新拋出。
                    singleAttemptWrapper.Execute();

                    // 執行成功，跳出迴圈
                    success = true;
                }
                catch(Exception ex)
                {
                    // 捕獲所有未被單次 Wrapper 處理的異常 (表示操作失敗)
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
                        // 最後一次嘗試失敗
                        Console.WriteLine("[FATAL] 所有重試次數已用盡，操作最終失敗。");
                    }
                }
            }

            return success;
        }
    }
}
