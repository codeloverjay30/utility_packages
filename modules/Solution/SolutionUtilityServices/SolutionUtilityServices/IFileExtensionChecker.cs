using System;
using System.Collections.Generic;
using System.Text;

namespace SolutionUtilityServices
{
    public interface IFileExtensionChecker
    {
        bool IsConfiguration(string filePath);
        bool IsDocument(string filePath);
        bool IsSolution(string filePath);
        bool IsProject(string filePath);
        bool IsProgrammingLanguage(string filePath);
        bool IsText(string filePath);
        bool NeedsToBeReplaced(string filePath);
    }
}
