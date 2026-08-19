using System.IO.Abstractions;
using GlobPatternUtilityServices.Abstractions;
using GlobPatternUtilityServices.Models;
using Microsoft.Extensions.FileSystemGlobbing;

namespace GlobPatternUtilityServices;

/// <summary>
/// Implements a highly optimized, top-down glob pattern matcher using ReadOnlySpan to avoid memory allocations.
/// </summary>
public class GitIgnoreStyleMatcher : IGlobPatternMatcher
{
    private readonly IFileSystem _fileSystem;
    private readonly List<GlobRuleEntry> _rules;
    private readonly Dictionary<string, CompiledGlobRule> _compiledRules;

    /// <summary>
    /// Initializes a new instance of the <see cref="GitIgnoreStyleMatcher"/> class and pre-compiles matchers to maximize throughput.
    /// </summary>
    /// <param name="fileSystem">The file system abstraction to use for file operations.</param>
    /// <param name="rules">The list of ordered glob rules.</param>
    /// <exception cref="ArgumentNullException">Thrown when rules is null.</exception>
    public GitIgnoreStyleMatcher(
        IFileSystem fileSystem,
        List<GlobRuleEntry> rules
    )
    {
        // Defensive Check
        ArgumentNullException.ThrowIfNull(fileSystem);
        ArgumentNullException.ThrowIfNull(rules);
        
        _fileSystem = fileSystem;
        _rules = rules;
        _compiledRules = new Dictionary<string, CompiledGlobRule>(rules.Count);

        PreCompileRules();
    }

    /// <summary>
    /// Pre-compiles glob rules using ReadOnlySpan to parse without substrings, avoiding heap allocation during setup.
    /// </summary>
    private void PreCompileRules()
    {
        foreach (var rule in _rules)
        {
            if (string.IsNullOrEmpty(rule.Pattern))
            {
                continue;
            }

            ReadOnlySpan<char> patternSpan = rule.Pattern.AsSpan();
            bool isInverse = false;

            // Check for inverse flag using Span extension method
            if (patternSpan.StartsWith("!".AsSpan(), StringComparison.Ordinal))
            {
                isInverse = true;
                patternSpan = patternSpan.Slice(1);
            }

            // 優化點 1：如果沒有驚嘆號，直接沿用 rule.Pattern 字串，完全零配置 (Zero-Allocation)
            // 如果有驚嘆號才需要轉成新字串
            string cleanPattern = isInverse ? patternSpan.ToString() : rule.Pattern;

            // 修正點 2：Microsoft 的 Globbing 套件內部只吃正斜線 "/"，在此處將反斜線統一替換
            cleanPattern = cleanPattern.Replace('\\', '/');

            var globbingMatcher = new Microsoft.Extensions.FileSystemGlobbing.Matcher();
            globbingMatcher.AddInclude(cleanPattern);

            // Replicating descendants logic for cascading to grandchildren (e.g., bin/ becomes bin/**/*)
            if (!cleanPattern.EndsWith("/**/*", StringComparison.Ordinal) &&
                !cleanPattern.EndsWith("/*", StringComparison.Ordinal))
            {
                string folderPattern = cleanPattern.EndsWith("/", StringComparison.Ordinal)
                    ? $"{cleanPattern}**/*"
                    : $"{cleanPattern}/**/*";

                globbingMatcher.AddInclude(folderPattern);
            }

            // 將編譯好的實體與計算出的真實 IsInverse 存入字典
            _compiledRules[rule.Pattern] = new CompiledGlobRule(globbingMatcher, isInverse);
        }
    }

    /// <inheritdoc />
    public void ProcessDirectory(IDirectoryInfo rootDirectory)
    {
        // Defensive Check
        if (rootDirectory == null) throw new ArgumentNullException(nameof(rootDirectory), "Root directory cannot be null.");
        if (!rootDirectory.Exists) throw new DirectoryNotFoundException($"The directory {rootDirectory.FullName} does not exist.");

        // Top-down search through file system
        var allFiles = rootDirectory.GetFiles("*", SearchOption.AllDirectories);

        foreach (var file in allFiles)
        {
            string relativePath = _fileSystem.Path.GetRelativePath(rootDirectory.FullName, file.FullName);

            string normalizedRelativePath = relativePath.Replace('\\', '/');

            // Top-Down, Lazy Search (First match stops further rule evaluation)
            foreach (var rule in _rules)
            {
                if (!_compiledRules.TryGetValue(rule.Pattern, out var compiledRule))
                {
                    continue;
                }

                var result = compiledRule.Matcher.Match(normalizedRelativePath);

                if (result.HasMatches)
                {
                    if (!rule.IsInverse)
                    {
                        // Match found! Apply to entry and descendants, then halt lower processing (Lazy search)
                        rule.ActionStrategy?.Execute(file);
                        break;
                    }
                    else
                    {
                        // Inverse rule matched (!): Explicitly excluded from action, halt processing for this file 
                        break;
                    }
                }
            }
        }
    }
}
    