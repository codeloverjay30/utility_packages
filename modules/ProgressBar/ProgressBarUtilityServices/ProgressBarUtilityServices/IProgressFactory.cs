using System;
using System.Collections.Generic;
using System.Text;

namespace ProgressBarUtilityServices
{
    public interface IProgressFactory
    {
        ITaskProgressTracker CreateTracker(string description);
    }
}
