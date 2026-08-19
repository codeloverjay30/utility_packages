using System;
using System.Collections.Generic;
using System.Text;

namespace ProgressBarUtilityServices
{
    public interface ITaskProgressTracker : IDisposable
    {
        void Update(double percentage , string? message = null);
        void Complete(string? message = null);
    }
}
