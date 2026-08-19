using System;
using System.Collections.Generic;


namespace ExceptionWrappers.Utilities
{
    public class GenericExceptionWrapper<T>
    {

        public Func<T> TryAction { get; protected set; }
        public Action FinallyAction { get; protected set; }

        public Dictionary<Type , Action<Exception>> ExceptionHandlers { get; protected set; }

        public GenericExceptionWrapper(
            Func<T> tryAction ,
            Action finallyAction = null,
            Dictionary<Type , Action<Exception>> exceptionHandlers = null
        )
        {
            this.TryAction = tryAction;
            this.FinallyAction = finallyAction;
            this.ExceptionHandlers = exceptionHandlers ?? new Dictionary<Type, Action<Exception>>();

            this.ValidateConstraints();
        }

        #region private method
        private void ValidateConstraints()
        {
            if(this.TryAction == null)
            {
                throw new ArgumentNullException(nameof(this.TryAction));
            }
        }
        #endregion
        public T Execute(
            T defaultReturnValue = default(T)
        )
        {
            if(this.ExceptionHandlers == null || this.ExceptionHandlers.Count <= 0)
            {
                this.ExceptionHandlers = new Dictionary<Type , Action<Exception>>();
            }

            try
            {
                // 1.執行步驟
                return TryAction();
            }
            catch(Exception ex)
            {
                // 2. 尋找最匹配的異常處理器
                // 我們查找所有註冊的類型，這些類型是 'ex' 類型本身或其基類。
                // 這裡使用 FirstOrDefault 選擇最接近的異常類型，並執行它。
                // 為了簡潔，我們只檢查精確匹配或基類匹配。
                var handlerEntry = this.ExceptionHandlers
                    .Where(pair => pair.Key.IsAssignableFrom(ex.GetType()))
                    .OrderByDescending(pair => GetExceptionHierarchyDepth(pair.Key))
                    .FirstOrDefault();

                if(handlerEntry.Key != null)
                {
                    // 3. 找到專屬 Handler：執行該 Handler
                    // 執行特定的處理邏輯 (例如：日誌記錄、重試等)
                    handlerEntry.Value(ex);
                    return defaultReturnValue;
                }
                else
                {
                    // 4. 沒有找到 Handler：拋出異常
                    // 如果沒有註冊任何處理器，或異常類型不匹配任何註冊的類型，則重新拋出異常。
                    throw;
                }
            }
        }

        #region private method
        // 輔助函式：計算異常類型的繼承深度，用於排序找到最特定的 Handler
        private static int GetExceptionHierarchyDepth(Type exceptionType)
        {
            int depth = 0;
            Type currentType = exceptionType;
            while (currentType != null && currentType != typeof(object))
            {
                depth++;
                currentType = currentType.BaseType;
            }
            return depth;
        }
        #endregion
    }
}
