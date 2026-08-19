using System;
using System.IO.Abstractions;
using System.Runtime.Loader;
using System.Threading.Tasks;
using VscUtilityServices.Core.Models;

namespace VscUtilityServices.Engines;

/// <summary>
/// Defines the defensive contract for task execution strategies across different programming languages.
/// </summary>
public interface ITaskExecutor
{
    /// <summary>
    /// Executes the specified target task within a defensive context.
    /// </summary>
    /// <param name="script">The script artifact containing execution details and metadata.</param>
    /// <param name="taskDefinition">The task definition extracted from the JSON5 configuration.</param>
    /// <returns>A task representing the asynchronous execution flow.</returns>
    Task ExecuteAsync(
        Script script,
        TaskDefinition taskDefinition
    );
}