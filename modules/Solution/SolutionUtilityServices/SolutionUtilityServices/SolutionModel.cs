using System;
using System.Collections.Generic;
using System.Text;

namespace SolutionUtilityServices
{
    /// <summary>
    /// POCO of solution
    /// </summary>
    public class SolutionModel
    {
        public string SolutionName { get; set; }
        public string RootPath { get; set; }
        public List<ProjectModel> Projects { get; set; }
    }
}
