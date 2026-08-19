using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace ListItemUitilityServices
{
    public enum ListItemsType
    {
        /// <summary>
        /// When create a menu, use alphabets as symbol
        /// </summary>
        [Description("alphabetic items")]
        ALPHABET = 0,

        /// <summary>
        /// When create a menu, use numbers as symbol
        /// </summary>
        [Description("numbered items")]
        NUMBER = 1,
    }
}
