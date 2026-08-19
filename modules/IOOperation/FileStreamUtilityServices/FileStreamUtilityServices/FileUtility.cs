using System.IO;
using System.Text;

namespace FileStreamUtilityServices
{
    public static class FileUtility
    {
        private static readonly Encoding DEFAULT_ENCODING = Encoding.UTF8;
        /// <summary>
        /// 以獨佔鎖定方式讀取檔案 (同步版本)，防止讀取期間內容被修改
        /// </summary>
        public static string ReadWithLock(
            string filePath,
            Encoding encoding = null
        )
        {
            if(string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("Path can niether be null nor empty." , nameof(filePath));
            }

            if(!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File does NOT exists: {filePath}");
            }

            encoding ??= DEFAULT_ENCODING;

            // FileMode.Open: 打開現有檔案
            // FileAccess.Read: 我只讀取
            // FileShare.None: 重要！這會鎖定檔案，讀取期間不允許其他程序(如 VSC 存檔)讀取或寫入
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
            using (var reader = new StreamReader(fs,encoding))
            {
                return reader.ReadToEnd();
            }
        }

        /// <summary>
        /// 以獨佔鎖定方式讀取檔案 (非同步版本)
        /// </summary>
        public static async Task<string> ReadWithLockAsync(
            string filePath ,
            Encoding encoding = null
        )
        {
            if(string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("Path can niether be null nor empty." , nameof(filePath));
            }

            if(!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File does NOT exists: {filePath}");
            }
            // 預設使用 UTF8
            encoding ??= DEFAULT_ENCODING;

            // FileShare.None 確保讀取期間其他程序無法存取
            using var fs = new FileStream(
                filePath ,
                FileMode.Open ,
                FileAccess.Read ,
                FileShare.None ,
                bufferSize: 4096 ,
                useAsync: true
            );

            using var reader = new StreamReader(fs , encoding);
            return await reader.ReadToEndAsync();
        }
    }
}
