using PostSharp.Aspects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace PostSharpAttributesUtilityService
{
    [Serializable]
    public class LogInteractionAttribute : OnMethodBoundaryAspect
    {
        // PostSharp 會自動注入此屬性，你可以透過反射或傳遞方式取得 Logger
        public override void OnEntry(MethodExecutionArgs args)
        {
            // 取得方法參數，例如 rx, ry 或 ClickJob
            var arguments = args.Arguments;


            // 取得方法名稱
            var methodName = args.Method.Name;

            // 取得命名空間
            var ns = args.Method.DeclaringType?.Namespace ?? "UnknownApp";

            string detail = JsonSerializer.Serialize(arguments);

            // TODO: 想要列印到日誌(根據組態設定，並非寫死)，其等級為LogLevel.Information，而不是列印到Console上

            // Console.WriteLine($"[{ns}] [{methodName}] {methodName} executed successfully with args: {detail}");
        }

        // PostSharp 會自動注入此屬性，你可以透過反射或傳遞方式取得 Logger
        public override void OnSuccess(MethodExecutionArgs args)
        {
            // 取得方法參數，例如 rx, ry 或 ClickJob
            var arguments = args.Arguments;


            // 取得方法名稱
            var methodName = args.Method.Name;

            // 取得命名空間
            var ns = args.Method.DeclaringType?.Namespace ?? "UnknownApp";

            string detail = JsonSerializer.Serialize(arguments);

            // TODO: 想要列印到日誌(根據組態設定，並非寫死)，其等級為LogLevel.Information，而不是列印到Console上

            // Console.WriteLine($"[{ns}] [{methodName}] {methodName} executed successfully with args: {detail}");
        }

        private void ExecuteLog(MethodExecutionArgs args , string status)
        {
            // 取得動態配置的 Logger (例如從您現有的 SerilogHelper 取得)
            var logger = LogAccessor.CurrentLogger;
            if(logger == null) return;

            var method = args.Method;
            var ns = method.DeclaringType?.Namespace ?? "Unknown";
            var className = method.DeclaringType?.Name ?? "Unknown";

            // 序列化參數 (已在 LogInteractionAttribute 中展示過)
            string detail = System.Text.Json.JsonSerializer.Serialize(args.Arguments);

            // 根據場景決定呼叫 Source Generator 產生的哪個方法
            switch(Scenario)
            {
                case LogScenario.Debug:
                    HighPerfLogger.LogDebug(logger , ns , className , method.Name , status , detail);
                    break;
                case LogScenario.Warning:
                    HighPerfLogger.LogWarning(logger , className , method.Name , status , detail);
                    break;
                default:
                    HighPerfLogger.LogStandard(logger , ns , className , method.Name , status , detail);
                    break;
            }
        }
    }
}
