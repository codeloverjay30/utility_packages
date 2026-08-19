using IOOperationUtilityServices;
using MiniExcelLibs;
using OfficeUtilityServices.Utilities.Excel;
using System;
using System.Collections.Generic;
using System.Text;

namespace OfficeUtilityService.Utilities.Excel
{
    public class VariableDumper : IVariableDumper
    {
        /// <summary>
        /// 試圖專門將資料寫入XLSX檔
        /// </summary>
        /// <typeparam name="T">資料類型</typeparam>
        /// <param name="fullName">檔案(絕對)路徑</param>
        /// <param name="data">要寫入的資料</param>
        /// <returns>成功狀態</returns>
        public bool Dump<T>(string fullName , T data)
        {
            try
            {
                if(data == null)
                {
                    throw new ArgumentException("要寫入的資料不能為null");
                }
                // 取得安全路徑(針對超長路徑)
                /// fullName = FileHandler.GetSafeLongPath(fullName);
                FileHandler.TryToOverwriteFile(fullName);
                MiniExcel.SaveAs(
                    path: fullName ,
                    value: data ,
                    excelType: ExcelType.XLSX , // 寫成XLSX檔 
                    overwriteFile: true // 覆寫檔案(若存在)
                );
                return true;
            }
            catch(Exception ex)
            {
                // Logic for Exception handling
                // ...
                return false;
            }
        }
    }
}
