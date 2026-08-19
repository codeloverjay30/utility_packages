using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using FileStreamUtilityServices;

namespace FileStreamUtilityServices.Tests
{
    public class FileUtilityTests : IDisposable
    {
        private readonly string _tempFilePath;

        public FileUtilityTests()
        {
            // 初始化：建立一個暫時測試檔案
            _tempFilePath = Path.Combine(Path.GetTempPath() , $"{Guid.NewGuid()}.txt");
            File.WriteAllText(_tempFilePath , "Hello World - 測試內容");
        }

        [Fact]
        public async Task ReadWithLockAsync_ShouldReadContentSuccessfully()
        {
            // Act
            string content = await FileUtility.ReadWithLockAsync(_tempFilePath);

            // Assert
            Assert.Equal("Hello World - 測試內容" , content);
        }

        [Fact]
        public void ReadWithLock_FileNotFound_ShouldThrowException()
        {
            // Arrange
            string nonExistentPath = "C:\\NonExistentFile.txt";

            // Act & Assert
            Assert.Throws<FileNotFoundException>(() => FileUtility.ReadWithLock(nonExistentPath));
        }

        [Fact]
        public void ReadWithLock_FileIsAlreadyLocked_ShouldThrowIOException()
        {
            // Arrange: 先手動用一個獨佔流鎖住檔案
            using var fs = new FileStream(_tempFilePath , FileMode.Open , FileAccess.ReadWrite , FileShare.None);

            // Act & Assert: 嘗試讀取應該會因為檔案被鎖定而失敗
            Assert.Throws<IOException>(() => FileUtility.ReadWithLock(_tempFilePath));
        }

        public void Dispose()
        {
            // 清理：測試結束後刪除暫時檔案
            if(File.Exists(_tempFilePath))
            {
                File.Delete(_tempFilePath);
            }
        }
    }
}
