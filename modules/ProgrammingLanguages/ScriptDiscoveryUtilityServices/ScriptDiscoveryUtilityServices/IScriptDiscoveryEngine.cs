namespace ScriptDiscoveryUtilityServices;

public interface IScriptDiscoveryEngine
{
    string? LocateMethodSourcePath(
       string rootDirectory,
       string targetMethodName,
       string programmingLanguage
   );
}
