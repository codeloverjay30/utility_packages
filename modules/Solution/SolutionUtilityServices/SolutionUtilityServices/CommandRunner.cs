using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace SolutionUtilityServices
{
    /// <summary>
    /// Command executor
    /// </summary>
    public class CommandRunner : ICommandRunner
    {
        public string RunCommandWithOutput(
            string cmd ,
            string args
        )
        {
            var psi = new ProcessStartInfo(cmd , args)
            {
                RedirectStandardOutput = true ,
                UseShellExecute = false ,
                CreateNoWindow = true
            };
            using var process = Process.Start(psi);
            return process?.StandardOutput.ReadToEnd() ?? string.Empty;
        }

        public void RunCommand(
            string cmd ,
            string args ,
            string workingDir
        )
        {
            var psi = new ProcessStartInfo(cmd , args)
            {
                WorkingDirectory = workingDir ,
                UseShellExecute = false ,
                CreateNoWindow = true
            };
            Process.Start(psi)?.WaitForExit();
        }
    }
}
