using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace ObfuscationServices;

/// <summary>
/// dynamic IL polymorphism generator
/// </summary>
public class PolymorphicGenerator
{
    public delegate int AddDelegate(int a, int b);

    /// <summary>
    /// use dynamic IL polymorphism engine to add two numbers
    /// </summary>
    /// <returns></returns>
    private static AddDelegate _CreatePolymorphicAdd()
    {
        // create 
        DynamicMethod dynamicAdd = new DynamicMethod(
            "DynamicAdd_" + Guid.NewGuid().ToString("N"),
            typeof(int),
            new[] { typeof(int), typeof(int) }
        );

        ILGenerator il = dynamicAdd.GetILGenerator();
        Random rand = new Random();

        // inser number of random Nop instructions
        int junkCount = rand.Next(1, 5);
        for (int i = 0; i < junkCount; i++)
        {
            il.Emit(OpCodes.Nop);
        }

        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldarg_1);
        il.Emit(OpCodes.Add);
        il.Emit(OpCodes.Ret);

        // 4. 編譯成委派 (Delegate)
        return (AddDelegate)dynamicAdd.CreateDelegate(typeof(AddDelegate));
    }

    public AddDelegate CreatePolymorphicAdd()
    {
        return _CreatePolymorphicAdd();
    }
}
