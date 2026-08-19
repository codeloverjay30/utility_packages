using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ObfuscationServices;

public class ExpressionPolymorphism
{
    /// <summary>
    /// 用Expression Trees進行動態邏輯變換
    /// </summary>
    /// <returns></returns>
    public static Func<int, bool> GenerateValidator()
    {
        // 定義參數：int input
        ParameterExpression inputParam = Expression.Parameter(typeof(int), "input");

        // 多態：隨機選擇一種邏輯來驗證「是否大於 100」
        Expression comparison;
        int seed = new Random().Next(0, 3);

        switch (seed)
        {
            case 0:
                // 邏輯 A: input > 100
                comparison = Expression.GreaterThan(inputParam, Expression.Constant(100));
                break;
            case 1:
                // 邏輯 B: (input - 101) >= 0
                comparison = Expression.GreaterThanOrEqual(
                    Expression.Subtract(inputParam, Expression.Constant(101)),
                    Expression.Constant(0));
                break;
            default:
                // 邏輯 C: 100 < input
                comparison = Expression.LessThan(Expression.Constant(100), inputParam);
                break;
        }

        // 將運算式編譯成 Lambda
        var lambda = Expression.Lambda<Func<int, bool>>(comparison, inputParam);

        // 編譯成執行碼 (這在幕後其實也會產生 IL)
        return lambda.Compile();
    }
}
    
