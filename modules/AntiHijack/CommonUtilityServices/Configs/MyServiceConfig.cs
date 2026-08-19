using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonUtilityService.Configs
{
    public class MyServiceConfig : IMyServiceConfig
    {
        [Required(AllowEmptyStrings = false)]
        public string SecureKeyId { get; set; } = null!;

        [Required(AllowEmptyStrings = false)]
        public string ConnectionString { get; set; } = null!;

        [Required(AllowEmptyStrings = false)]
        public string ApiUrl { get; set; } = null!;
    }
}
