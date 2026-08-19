using System;
using System.Collections.Generic;

namespace ExceptionWrappers.Utilities
{
    public class NonGenericExceptionWrapper
    {
        public Action TryAction { get; protected set; }
        public Action FinallyAction { get; protected set; }

        public Dictionary<Type , Action<Exception>> ExceptionHandlers { get; protected set; }

        public NonGenericExceptionWrapper(
            Action tryAction ,
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

        /// <summary>
        /// (Void 版本) 安全地執行一個沒有回傳值的動作。
        /// </summary>
        public void Execute()
        {
            var genericExceptionWrapper = 
                new GenericExceptionWrapper<bool>(() =>
                {
                    this.TryAction();
                    return true;
                },
                this.FinallyAction,
                this.ExceptionHandlers
                );

            genericExceptionWrapper.Execute(true);
        }
    }
}
