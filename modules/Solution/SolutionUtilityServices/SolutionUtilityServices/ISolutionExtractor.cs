using System;
using System.Collections.Generic;
using System.Text;

namespace SolutionUtilityServices
{
    public interface ISolutionExtractor
    {
        void ExtractSpecificProjects(List<ProjectModel> sourceProjects);
        void ExtractWholeSolution();
    }
}
