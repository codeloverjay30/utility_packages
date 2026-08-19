using System;
using System.Collections.Generic;
using System.Text;

namespace SolutionUtilityServices
{
    public interface ICommandRunner
    {
        public string RunCommandWithOutput(
            string cmd ,
            string args
        );
        public void RunCommand(
            string cmd ,
            string args ,
            string workingDir
        );
    }
}
