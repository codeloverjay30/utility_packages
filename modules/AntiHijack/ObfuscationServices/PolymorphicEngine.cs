using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ObfuscationServices;

public class PolymorphicEngine<TInput, TOutput>: IPolymorphicEngine<TInput,TOutput>
{
    private readonly List<Expression<Func<TInput, TOutput>>> _variants = new();
    private readonly Random _random = new();

    public void RegisterVariant(Expression<Func<TInput, TOutput>> variant)
    {
        _variants.Add(variant);
    }

    public TOutput Execute(TInput input)
    {
        if (_variants.Count == 0) throw new InvalidOperationException("未註冊任何邏輯變體");

        // 隨機選擇一個變體進行編譯與執行
        // 進階：可以在這裡加入 ExpressionVisitor 來進一步混淆
        var selectedIndex = _random.Next(_variants.Count);
        var compiled = _variants[selectedIndex].Compile();

        return compiled(input);
    }
}
    
