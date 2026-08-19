using System;
using System.Collections.Generic;
using System.Text;

namespace ExceptionHandlingUtilityServices
{
    public class ExceptionUtilityService(Exception ex): IExceptionUtilityService
    {
        /// <summary>
        /// Flatten the inner exception and process all of them.
        /// </summary>
        /// <param name="action"></param>
        public void FlattenAndProcess(Action<Exception> action)
        {
            if(ex is AggregateException ae)
            {
                foreach(var innerEx in ae.Flatten().InnerExceptions)
                {
                    action(innerEx);
                }
            }
            else
            {
                action(ex);
            }
        }
    }
}
