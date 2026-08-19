using NetRuntimeUtilityServices;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetRuntimeUtilityServices.Tests
{
    public class TestModel
    {
        // 假設我們測試一個需要目前環境運行的屬性
        [RequiresRuntime(8,0 , "WINDOWS" , "LINUX" , "OSX")]
        public string EnvironmentSensitiveProperty { get; set; } = "SomeValue";
    }
}
