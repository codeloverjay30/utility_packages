using System;
using System.Collections.Generic;
using System.Text;

namespace OfficeUtilityServices.Utilities.Excel
{
    public interface IVariableDumper
    {
        bool Dump<T>(string path , T value);
    }
}
