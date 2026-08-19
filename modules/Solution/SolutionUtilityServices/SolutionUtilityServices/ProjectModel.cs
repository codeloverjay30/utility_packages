using System;
using System.Collections.Generic;
using System.Text;

namespace SolutionUtilityServices
{
    /// <summary>
    /// POCO of Project
    /// </summary>
    public class ProjectModel
    {
        public required string ProjectName { get; init; }
        public required string RootPath { get; init; }

        public required string RootNamespace { get; init; }
    }
}
