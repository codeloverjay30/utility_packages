using System.Reflection;
using System.Text;
using FileNameUtilityFactories;

namespace LogNameUtilityFactories
{
    public partial class LogFullPathFactory : ILogFullPathFactory
    {
        private readonly ILogNameFactory _logNameFactory;
        private readonly string _baseDirectory;
        public LogFullPathFactory(
            string baseDirectory,
            ILogNameFactory logNameFactory
        )
        {
            ArgumentException.ThrowIfNullOrEmpty(baseDirectory);
            ArgumentNullException.ThrowIfNull(logNameFactory);

            if(!Directory.Exists(baseDirectory))
            {
                Directory.CreateDirectory(baseDirectory);
            }
            _baseDirectory = baseDirectory;
            _logNameFactory = logNameFactory;
        }
        public string Create()
        {
            var logFilename = _logNameFactory.Create();
            return Path.Combine(this._baseDirectory,logFilename);
        }
    }
}
