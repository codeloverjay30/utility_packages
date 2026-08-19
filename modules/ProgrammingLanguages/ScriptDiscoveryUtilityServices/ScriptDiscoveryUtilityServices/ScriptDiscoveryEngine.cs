using System.Collections.Concurrent;
using System.IO.Abstractions;
using ProgrammingLanguageUtilityServices;

namespace ScriptDiscoveryUtilityServices;

/// <summary>
/// Defensively parses file contents within the workspace to discover target routine boundaries without raw string false-positives.
/// </summary>
public class ScriptDiscoveryEngine : IScriptDiscoveryEngine
{
    private static readonly ConcurrentDictionary<string, ProgammingLanguageInfo> _defaultProgrammingLanguageInfoPatterns = ProgrammingLanguageHelper.GetDefaultProgrammingLanguagePatterns();
    private readonly IFileSystem _fileSystem;
    private readonly ISignatureUtilityService _signatureUtilityService;

    public ScriptDiscoveryEngine(
        IFileSystem fileSystem,
        ISignatureUtilityService signatureUtilityService
    )
    {
        ArgumentNullException.ThrowIfNull(fileSystem);
        ArgumentNullException.ThrowIfNull(signatureUtilityService);

        _fileSystem = fileSystem;
        _signatureUtilityService = signatureUtilityService;
    }

    /// <summary>
    /// Recursively scans the targeted directory to locate the exact source file containing the defined method or function signature.
    /// </summary>
    /// <param name="rootDirectory">The parent container root directory.</param>
    /// <param name="targetMethodName">The exact name of the method to look for.</param>
    /// <param name="programmingLanguage">Programming language boundary (csharp/python/fsharp).</param>
    /// <returns>The absolute path of the discovered file, or null if not found.</returns>
    public string? LocateMethodSourcePath(
        string rootDirectory,
        string targetMethodName,
        string programmingLanguage
    )
    {
        if (!_fileSystem.Directory.Exists(rootDirectory))
        {
            throw new DirectoryNotFoundException($"Root container target path '{rootDirectory}' does not exist.");
        }

        if (_defaultProgrammingLanguageInfoPatterns.TryGetValue(programmingLanguage, out var progammingLanguageInfo))
        {
            var extensionPattern = progammingLanguageInfo.FileExtension;
            // Recursively search files under the parent container root path
            var files = _fileSystem.Directory.GetFiles(rootDirectory, extensionPattern, SearchOption.AllDirectories);

            foreach (var filePath in files)
            {
                string content = _fileSystem.File.ReadAllText(filePath);
                var signatureInfo = new SignatureInfo
                {
                    MethodName = targetMethodName,
                };
                if (_signatureUtilityService.IsSignatureMatched(content, programmingLanguage, signatureInfo))
                {
                    return filePath;
                }
            }

            return null;
        }
        throw new NotSupportedException($"The programming language '{programmingLanguage}' is not supported by the discovery engine.");
    }
}
 

