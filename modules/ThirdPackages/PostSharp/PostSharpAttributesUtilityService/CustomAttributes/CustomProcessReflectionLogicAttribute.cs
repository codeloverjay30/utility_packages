namespace CustomAttributes
{ 
    [AttributeUsage(AttributeTargets.All)]
    public class CustomProcessReflectionLogicAttribute(string config) : CustomProcessReflectionLogicBaseAttribute(config)
    {

    }
}
