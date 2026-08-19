using System;
using System.Collections.Generic;
using System.Text;

namespace SolutionUtilityServices.Runners.Models
{
    public class ExtractorConfig
    {
        public bool IsExtractWholeSolution { get; init; }
        public SolutionModel SourceSolution { get; init; }
        public SolutionModel TargetSolution { get; init; }
    }
}
