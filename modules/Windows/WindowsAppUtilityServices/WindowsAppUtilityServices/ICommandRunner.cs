using System;
using System.Collections.Generic;
using System.Text;

namespace WindowsAppUtilityServices;

public interface ICommandRunner
{
    /// <summary>
    /// Execute the command <paramref name="command"/>
    /// </summary>
    /// <param name="command">command</param>
    void ExecuteCommand(string command);
}
    
