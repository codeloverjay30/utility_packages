using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonUtilityService.Configs
{
    public interface IMyServiceConfig
    {
        string SecureKeyId { get; }
        string ConnectionString { get; }
        string ApiUrl { get; }
    }
}
