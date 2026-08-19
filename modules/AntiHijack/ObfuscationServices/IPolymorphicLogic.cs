using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ObfuscationService.Utilities
{
    public interface IPolymorphicLogic<TInput, TOutput>
    {
        // 使用者實作此方法來提供不同的邏輯變體
        Expression<Func<TInput , TOutput>> GetVariant();
    }
}
