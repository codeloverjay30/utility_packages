using CustomAttributes;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace CustomAttributes
{
    public class CustomReflectionLogicBaseAttributeScanner<TAtrribute>(
        TAtrribute attributeToBeScanned
    )
        where TAtrribute : CustomProcessReflectionLogicBaseAttribute
    {
        private readonly TAtrribute _attributeToBeScanned = attributeToBeScanned;
        public void ProcessActionByAttribute(
            Type type,
            Action func
        )
        {
            // Get all public instance methods
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

            foreach(var method in methods)
            {
                // 1. Try to get the attribute from the Method (High Priority)
                var attribute = method.GetCustomAttribute<TAtrribute>();

                // 2. If not found on the method, try the Class (Low Priority)
                if(attribute == null)
                {
                    attribute = type.GetCustomAttribute<TAtrribute>();
                }

                // 3. If an attribute exists in either location, execute the logic
                if(attribute != null)
                {
                    func.Invoke();
                }
            }
        }

        private void ExecuteAspectLogic(MethodInfo method , string config)
        {
            Console.WriteLine($"Applying logic to {method.Name} with config: {config}");
            // Your MSIL manipulation or wrapper logic goes here
        }
    }
}
