using System.IO.Abstractions;
using FSharp.Compiler.Interactive;
using Microsoft.FSharp.Core;
using VscUtilityServices.Core.Models;

namespace VscUtilityServices.Engines;

/// <summary>
/// Controls F# task execution lifecycle with structural type reflections.
/// </summary>
public class IsolatedFsharpExecutor : ITaskExecutor
{
    private readonly IFileSystem _fileSystem;

    public IsolatedFsharpExecutor(
        IFileSystem fileSystem
    )
    {
        ArgumentNullException.ThrowIfNull(fileSystem);
        _fileSystem = fileSystem;
    }
    /// <inheritdoc />
    public Task ExecuteAsync(
        Script script,
        TaskDefinition taskDefinition
    )
    {
        // Defensive Guard: Validate script and task definition integrity before execution
        if (script == null)
        {
            throw new ArgumentNullException(nameof(script), "Script artifact cannot be null.");
        }

        if (taskDefinition == null)
        {
            throw new ArgumentNullException(nameof(taskDefinition), "Task definition cannot be null.");
        }

        if (!_fileSystem.File.Exists(script.Path))
        {
            throw new FileNotFoundException($"Target F# script not found at specified path: {script.Path}", script.Path);
        }

        if (string.IsNullOrWhiteSpace(taskDefinition.TaskName))
        {
            throw new ArgumentException("Task name cannot be null or empty.", nameof(taskDefinition.TaskName));
        }
        // For demonstration, we will assume the task name corresponds to a function in the F# script.
        // In a real implementation, you would have more complex logic to map task definitions to script functions.
        var tempResult = ExecuteFsharpFunctionAsync(script.Path, taskDefinition.TaskName);
        return Task.CompletedTask;
    }

    public async Task<object> ExecuteFsharpFunctionAsync(
        string scriptPath,
        string functionName,
        params object[] args
    )
    {
        if (string.IsNullOrWhiteSpace(scriptPath))
            throw new ArgumentException("Script path cannot be null or empty.", nameof(scriptPath));

        if (string.IsNullOrWhiteSpace(functionName))
            throw new ArgumentException("Function name cannot be null or empty.", nameof(functionName));

        // Defensive Check: Verify file existence via abstracted file system
        if (!_fileSystem.File.Exists(scriptPath))
        {
            throw new FileNotFoundException($"Target F# script not found at specified path: {scriptPath}", scriptPath);
        }

        // Setup F# Interactive (FSI) Session configuration
        var baseConfig = Shell.FsiEvaluationSession.GetDefaultConfiguration();
        var argv = new[] { "fsi.exe", "--noninteractive" };

        using var inStream = new StringReader(string.Empty);
        using var outStream = new StringWriter();
        using var errStream = new StringWriter();

        var collectionOption = new FSharpOption<bool>(true);
        // Initialize isolated evaluation session
        using var session = Shell.FsiEvaluationSession.Create(
            baseConfig,
            argv,
            inStream,
            outStream,
            errStream,
            collectible: collectionOption,
            legacyReferenceResolver: null
        ); // collectible: true enables GC to collect the generated assembly

        try
        {
            // 1. Read script content via abstracted file system
            string scriptContent = await _fileSystem.File.ReadAllTextAsync(scriptPath).ConfigureAwait(false);

            // 2. Evaluate script into the interaction context
            var evalResult = session.EvalInteractionNonThrowing(
                scriptContent,
                cancellationToken: default
            );
            if (evalResult.Item1 is FSharpChoice<FSharpOption<Shell.FsiValue>?, Exception> errorChoice)
            {
                throw new InvalidOperationException($"Failed to evaluate F# script base structure. Errors: {errStream.ToString()}. Inner Exception: {errorChoice.Tag}");
            }

            // 3. Defensive lookup of the requested function symbol
            var fsiValue = session.EvalExpressionNonThrowing(functionName);
            if (fsiValue.Item1 is FSharpChoice<FSharpOption<Shell.FsiValue>, Exception>.Choice2Of2 expressionError)
            {
                throw new MissingMethodException($"The function '{functionName}' could not be resolved or found within the F# script context.", expressionError.Item);
            }

            var optValue = fsiValue.Item1 as FSharpChoice<FSharpOption<Shell.FsiValue>, Exception>.Choice1Of2;
            if (optValue == null || FSharpOption<Shell.FsiValue>.get_IsNone(optValue.Item))
            {
                throw new MissingMethodException($"The function '{functionName}' evaluated to none or is unavailable.");
            }

            object dynamicFunction = optValue.Item.Value.ReflectionValue
                                     ?? throw new InvalidOperationException($"The function '{functionName}' does not have a valid reflection value for invocation.");

            // 4. Invoke F# function via reflection or standard invocation
            object? result = InvokeFSharpFunction(dynamicFunction, args);
            return result;
        }
        catch (Exception ex) when (!(ex is OperationCanceledException || ex is FileNotFoundException || ex is MissingMethodException))
        {
            // Defensive Rollback/Wrap to protect caller pipeline from untrusted script crashes
            throw new InvalidOperationException($"Critical failure occurred while executing F# script '{scriptPath}' inside target function '{functionName}'. StdErr: {errStream}", ex);
        }
    }

    /// <summary>
    /// Curried function invocation handler for F# dynamic values.
    /// </summary>
    private static object? InvokeFSharpFunction(object fsharpFunc, object[] args)
    {
        if (args == null || args.Length == 0)
        {
            return fsharpFunc;
        }

        object currentTarget = fsharpFunc;

        // F# functions with multiple parameters are curried by default (e.g., x -> y -> z)
        // We iteratively invoke the Invoke method for each argument.
        foreach (var arg in args)
        {
            var type = currentTarget.GetType();
            var invokeMethod = type.GetMethods()
                .FirstOrDefault(m => m.Name == "Invoke" && m.GetParameters().Length == 1);

            if (invokeMethod == null)
            {
                throw new InvalidCastException($"Failed to curry argument onto F# function. Object type '{type.FullName}' does not expose a single-parameter Invoke method.");
            }

            currentTarget = invokeMethod.Invoke(currentTarget, new[] { arg })
                            ?? throw new InvalidOperationException("F# curried invocation returned a null reference unexpectedly.");
        }

        return currentTarget;
    }
}
    
