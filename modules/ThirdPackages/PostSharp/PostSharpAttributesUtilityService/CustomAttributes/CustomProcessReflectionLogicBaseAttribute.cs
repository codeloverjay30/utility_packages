using System;
using System.Collections.Generic;
using System.Text;

namespace CustomAttributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method , AllowMultiple = false , Inherited = true)]
    public class CustomProcessReflectionLogicBaseAttribute: Attribute, ICustomProcessReflectionLogicAttribute
    {
        public string Configuration { get; set; }
        public CustomProcessReflectionLogicBaseAttribute(string config)
        {
            Configuration = config;
        }
    }
}
