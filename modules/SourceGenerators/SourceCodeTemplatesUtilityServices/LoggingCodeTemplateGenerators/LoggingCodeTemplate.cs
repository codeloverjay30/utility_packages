namespace LoggingCodeTemplateGenerators
{
    namespace LoggingCodeTemplateGenerators
    {
        public static class LoggingCodeTemplate
        {
            public static string BuildPartialClass(
                string serviceName ,         // 例如: "_loggingService"
                string staticServiceName ,   // 例如: "LoggingBridge"
                string methodNameToInvoke ,  // 例如: "TriggerLog"
                string ns ,
                string className ,
                string args ,                // 預先格式化好的參數字串，例如: "\"Taps\", \"Click\""
                bool useInterface)
            {
                // 根據 useInterface 決定最終的呼叫語法
                string callInstance = $"this.{serviceName}.{methodNameToInvoke}({args});";
                string callStatic = $"{staticServiceName}.{methodNameToInvoke}({args});";

                string callLogic = useInterface ? callInstance : callStatic;

                // 使用 C# 11+ 的原始字串字面量 (Raw String Literals) 處理縮進更漂亮
                return $@"
namespace {ns}
{{
    public partial class {className}
    {{
        private void __Generated_Log_Invoke()
        {{
            {callLogic}
        }}
    }}
}}";
            }
        }
    }

}
