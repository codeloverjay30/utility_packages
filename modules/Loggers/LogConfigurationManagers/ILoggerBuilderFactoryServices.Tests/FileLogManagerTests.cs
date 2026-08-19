using NUnit.Framework;

namespace ILoggerBuilderFactoryServices.Test
{

    [TestFixture]
    public class FileLogManagerTests
    {
        private string _testFilePath;

        [SetUp]
        public void Setup()
        {
            _testFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory , "test_log.txt");
        }

        [TearDown]
        public void Cleanup()
        {
            // 嘗試清理測試檔案
            if(File.Exists(_testFilePath))
            {
                try { File.Delete(_testFilePath); } catch { /* 忽略檔案占用 */ }
            }
        }

        [Test]
        public void Instance_ShouldNotBeNull()
        {
            Assert.That(FileLogManager.Instance , Is.Not.Null);
        }

        [Test]
        public void Initialize_ShouldCreateDirectoryAndFile()
        {
            // Act
            FileLogManager.Instance.Initialize(_testFilePath);
            FileLogManager.Instance.Enqueue("Test Message");

            // 等待背景任務寫入
            Thread.Sleep(500);

            // Assert
            Assert.That(File.Exists(_testFilePath) , Is.True);
        }
    }
}
