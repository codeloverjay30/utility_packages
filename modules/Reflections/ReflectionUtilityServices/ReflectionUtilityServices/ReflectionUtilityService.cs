using ExpressionTreeUtilityServices;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;

namespace ReflectionUtilityServices
{
    public class ReflectionUtilityService : IReflectionUtilityService
    {
        private IExpressionTreeUtilityService _expressionTreeUtilityService;
        private List<Func<object , object? [ ]? , object?>?> _fastDelegates = new();
        public List<Func<object , object? [ ]? , object?>?> FastDelegates => _fastDelegates;
        public Func<object , object? [ ]? , object?>? FastInvoke => FastDelegates.LastOrDefault();
        public ReflectionUtilityService(
            IExpressionTreeUtilityService expressionTreeUtilityService
        )
        {
           _expressionTreeUtilityService = expressionTreeUtilityService;
        }

        public void AddFastDelegates(IEnumerable<MethodInfo> methodInfos)
        {
            foreach(var methodInfo in methodInfos)
            {
                AddFastDelegate(methodInfo);
            }
        }

        public void AddFastDelegate(MethodInfo methodInfo)
        {
            var instanceParam = Expression.Parameter(typeof(object) , "instance");
            var argumentsParam = Expression.Parameter(typeof(object [ ]) , "args");

            // 處理靜態或實例呼叫
            Expression callExp;
            if(methodInfo.IsStatic)
            {
                callExp = Expression.Call(methodInfo , _expressionTreeUtilityService.CreateParameterExpressions(methodInfo , argumentsParam));
            }
            else
            {
                var castInstance = Expression.Convert(instanceParam , methodInfo.DeclaringType!);
                callExp = Expression.Call(castInstance , methodInfo , _expressionTreeUtilityService.CreateParameterExpressions(methodInfo , argumentsParam));
            }

            Func<object , object? [ ]? , object?> @delegate;

            // 處理 void 回傳
            if(methodInfo.ReturnType == typeof(void))
            {
                var action = Expression.Lambda<Action<object? , object? [ ]?>>(callExp , instanceParam , argumentsParam).Compile();
                @delegate = (inst , args) => { action(inst , args); return null; };
                _fastDelegates.Add(@delegate);
                return;
            }

            var castResult = Expression.Convert(callExp , typeof(object));
            @delegate = Expression.Lambda<Func<object? , object? [ ]? , object?>>(castResult , instanceParam , argumentsParam).Compile();
            _fastDelegates.Add(@delegate);
            return;
        }

        public object?[] BindArguments(
            ParameterInfo [ ] methodParams ,
            Dictionary<string , JsonElement> arguments
        )
        {
            var convertedArgs = new object? [ methodParams.Length ];

            for(int i = 0; i < methodParams.Length; i++)
            {
                var p = methodParams [ i ];
                if(arguments.TryGetValue(p.Name! , out var jsonVal))
                {
                    // 這裡可以加入快取 JsonSerializerOptions 以進一步優化
                    convertedArgs [ i ] = JsonSerializer.Deserialize(jsonVal.GetRawText() , p.ParameterType);
                }
                else if(p.HasDefaultValue)
                {
                    convertedArgs [ i ] = p.DefaultValue;
                }
            }
            return convertedArgs;
        }
    }
}
