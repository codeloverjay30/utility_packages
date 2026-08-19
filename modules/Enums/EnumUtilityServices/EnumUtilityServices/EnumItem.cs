using System;
using System.Collections.Generic;
using System.Text;

namespace EnumUtilityServices
{
    public class EnumItem
    {
        /// <summary>
        /// value of entry in enum.
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// entry name in enum
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// the description of the entry (i.e. the value of field in `[Description]` data annotations.
        /// </summary>
        public string Description { get; set; }
    }
}
