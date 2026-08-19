using FileNameUtilityFactories;
using System.Reflection;
using System.Text;

namespace LogNameUtilityFactories
{
    public class LogNameFactory : ILogNameFactory
    {
        private IProjectNameFactory _projectNameFactory;

        public LogNameFactory(
            IProjectNameFactory projectNameFactory
        )
        {
            ArgumentNullException.ThrowIfNull(projectNameFactory);
            _projectNameFactory = projectNameFactory;
        }
        public string Create()
        {
            string logFilename = string.Empty;
            var clientDeviceName = Environment.MachineName;
            logFilename = $"{clientDeviceName}_";
            if(_projectNameFactory.Assembly is not null){
                var projectNameFactory = _projectNameFactory;
                var (shortname, version) = projectNameFactory.Create();
                logFilename = $"{logFilename}{shortname}_{version}_";
            }
            else
            {
                logFilename = $"{logFilename}UnknownAssembly_";
            }
            var now = DateTime.UtcNow.ToString("yyyyMMddHHmm");
            logFilename = $"{logFilename}{now}.log";
            return logFilename;
        }
    }
}
