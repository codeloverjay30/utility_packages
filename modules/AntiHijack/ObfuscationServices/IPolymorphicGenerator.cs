namespace ObfuscationServices;

public interface IPolymorphicGenerator
{
    delegate int AddDelegate(int a, int b);
    AddDelegate CreatePolymorphicAdd();
}
