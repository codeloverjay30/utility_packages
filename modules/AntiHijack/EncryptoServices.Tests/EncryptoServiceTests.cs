using System;
using System.Security.Cryptography;
using System.Text;
using EncryptoUtilityServices;
using FluentAssertions;
using Moq;
using SharedUtilityServices;
using Xunit;

namespace EncryptoServices.Tests;

/// <summary>
/// Contains comprehensive defensive and high-performance unit tests for the <see cref="EncryptoService"/> class.
/// </summary>
public class EncryptoServiceTests : IDisposable
{
    private readonly MockRepository _mockRepository;
    private readonly Mock<IByteArrayPool> _mockByteArrayPool;
    private readonly EncryptoService _sut;
    private readonly byte[] _valid32ByteKey;

    /// <summary>
    /// Initializes a new instance of the <see cref="EncryptoServiceTests"/> class, setting up strict defensive boundaries.
    /// </summary>
    public EncryptoServiceTests()
    {
        // 鐵律2：使用 Strict 模式，嚴密防禦 Moq 內部因未配置導覽屬性所引發的平行時空副作用與隱蔽異常
        _mockRepository = new MockRepository(MockBehavior.Strict);
        _mockByteArrayPool = _mockRepository.Create<IByteArrayPool>();

        // 正確注入相依性 IByteArrayPool 至被測系統 (SUT)
        _sut = new EncryptoService(_mockByteArrayPool.Object);

        _valid32ByteKey = new byte[32];
        RandomNumberGenerator.Fill(_valid32ByteKey.AsSpan());
    }

    [Fact]
    public void Constructor_WhenByteArrayPoolIsNull_ShouldThrowArgumentNullExceptionWithCorrectMessage()
    {
        // Arrange
        IByteArrayPool? nullPool = null;

        // Act
        Action act = () => new EncryptoService(nullPool!);

        // Assert (鐵律1：必須使用 Action 攔截並透過 FluentAssertions 驗證真實的 Exception)
        act.Should().Throw<ArgumentNullException>()
           .Where( p => p.ParamName == "byteArrayPool");
    }

    [Fact]
    public void EncryptText_WhenKeyLengthIsInvalid_ShouldThrowArgumentException()
    {
        // Arrange & Act
        Action act = () =>
        {
            ReadOnlySpan<char> plainText = "SecureData123".AsSpan();
            ReadOnlySpan<byte> invalidKey = stackalloc byte[16]; // 錯誤的長度 (AES-128)
            _sut.EncryptText(plainText, invalidKey);
        };

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .Where(p=>p.ParamName == "keyBytes");
    }

    [Fact]
    public void EncryptText_WhenPlainTextIsEmpty_ShouldReturnEmptyStringImmediately()
    {
        // Arrange
        ReadOnlySpan<char> emptyText = ReadOnlySpan<char>.Empty;

        // Act
        string result = _sut.EncryptText(emptyText, _valid32ByteKey);

        // Assert
        result.Should().BeEmpty("Because an empty character span requires no cryptographic transformations.");
    }

    [Fact]
    public void EncryptText_WhenInputsAreValidAndUnderStackAllocThreshold_ShouldReturnValidBase64StringWithoutRentingArray()
    {
        // Arrange
        ReadOnlySpan<char> plainText = "DefensiveProgramming".AsSpan();
        // 由於長度小於 1024 字节，底層會走 stackalloc，此處不應 Setup Rent 與 Return (展現 MockBehavior.Strict 之威力)

        // Act
        string result = _sut.EncryptText(plainText, _valid32ByteKey);

        // Assert
        result.Should().NotBeNullOrEmpty();
        Action decodeAct = () => Convert.FromBase64String(result);
        decodeAct.Should().NotThrow("Because the output ciphertext must strictly conform to a valid Base64 architecture.");
    }

    /// <summary>
    /// Validates that when the payload magnitude marginally exceeds the stackallocation safety limits,
    /// the cryptographic workflow correctly rents and disposes memory arrays from the pool without leaving side-effects.
    /// </summary>
    [Fact]
    public void EncryptText_WhenPayloadIsLarge_ShouldRentAndReturnBuffersFromPoolCorrectly()
    {
        // Arrange
        // Defensive Design: 精準控制字串長度為 1050 字元。
        // 剛好超越 MaxStackAllocBytes (1024) 強迫走 Pool 分支，同時防範字串過巨引發 .NET 核心庫內部的二次配置副作用
        string targetLargeString = new string('A', 1050);
        ReadOnlySpan<char> plainText = targetLargeString.AsSpan();

        // Guard: 提供完美包覆 1050 與 1072 期待長度的偽造陣列，徹底與底層編碼溢位風險絕緣
        byte[] plainRentedArray = new byte[1500]; 
        byte[] cipherRentedArray = new byte[3000]; 

        // Rule 2: Harness SetupSequence to neutralize overlapping conditional matchers in concurrent execution scopes
        _mockByteArrayPool.SetupSequence(p => p.Rent(It.IsAny<int>()))
                          .Returns(plainRentedArray)
                          .Returns(cipherRentedArray);

        // Alignment Guard: Match the precise single-argument method compilation signature containing the implicit default parameter (false)
        _mockByteArrayPool.Setup(p => p.Return(It.Is<byte[]>(arr => object.ReferenceEquals(arr, plainRentedArray)), false))
                          .Verifiable();

        _mockByteArrayPool.Setup(p => p.Return(It.Is<byte[]>(arr => object.ReferenceEquals(arr, cipherRentedArray)), false))
                          .Verifiable();

        // Act
        string cipherBase64 = _sut.EncryptText(plainText, _valid32ByteKey);

        // Assert
        // Rule 1: FluentAssertions standard compliance check
        cipherBase64.Should().NotBeNullOrEmpty("Because a verified deep pooling cipher process must yield a legitimate block output.");
        
        Action act = () => Convert.FromBase64String(cipherBase64);
        act.Should().NotThrow("Because the encrypted output string structure must strictly conform to a clean Base64 standard.");
    }

    [Fact]
    public void DecryptText_WhenKeyLengthIsInvalid_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange && Act
        // 鐵律1：精準攔截只包含 SUT 核心行為的 Action
        Action act = () =>
        {
            // Arrange
            ReadOnlySpan<char> cipherText = "AnyBase64String==".AsSpan();
            // 防禦心法：將測試資料配置移出 Act 閉包，避免 ref struct 與 Lambda 產生生命週期干擾
            byte[] invalidKeyArray = new byte[16];
            ReadOnlySpan<byte> invalidKey = invalidKeyArray.AsSpan();

            _sut.DecryptText(cipherText, invalidKey);
        };
         
            // Assert
            // 修正預期 Message，必須與 .NET Core 內建的 ArgumentOutOfRangeException.ThrowIfNotEqual 訊息完全契合
        act.Should().Throw<ArgumentOutOfRangeException>()
           .Where(p =>p.ParamName == "keyBytes")
           .WithMessage("*must be equal to '32'*");
    }
    

    [Fact]
    public void DecryptText_WhenCipherTextIsEmpty_ShouldReturnEmptyStringImmediately()
    {
        // Arrange
        ReadOnlySpan<char> emptyCipher = ReadOnlySpan<char>.Empty;

        // Act
        string result = _sut.DecryptText(emptyCipher, _valid32ByteKey);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void DecryptText_WhenCipherTextIsCorruptedOrTooShort_ShouldThrowCryptographicException()
    {
        // Arrange &Act
        Action act = () =>
        {
            // 提供一個合法 Base64 格式但長度不足 16 位元組 (無法抽取 IV) 的字串
            string shortBase64 = Convert.ToBase64String(new byte[10]);
            ReadOnlySpan<char> cipherText = shortBase64.AsSpan();
            _sut.DecryptText(cipherText, _valid32ByteKey);
        };
        // Assert
        act.Should().Throw<CryptographicException>()
           .WithMessage("*Cipher payload is truncated or severely corrupted*");
    }

    [Fact]
    public void RoundTrip_EncryptAndDecrypt_ShouldRestoreOriginalTextPerfect_UnderDefensiveDesign()
    {
        // Arrange
        // 為了完整測試真實環境加解密迴圈，我們建立一個使用真實行為的實體進行驗證
        var realPool = new SharedByteArrayPool();
        var realSut = new EncryptoService(realPool);

        ReadOnlySpan<char> originalText = "TopSecret_Data_Within_DotNet10_Environment".AsSpan();

        // Act
        string encryptedBase64 = realSut.EncryptText(originalText, _valid32ByteKey);
        string decryptedText = realSut.DecryptText(encryptedBase64.AsSpan(), _valid32ByteKey);

        // Assert
        decryptedText.Should().Be(originalText.ToString(), "Because a reliable defensive cryptographic workflow must restore data integrity identically.");
    }

    /// <summary>
    /// Cleans up resources and strictly audits that all mocked interactions satisfied expectations.
    /// </summary>
    public void Dispose()
    {
        // 鐵律2：強迫在清理階段執行 VerifyAll，防範內部有多餘或非預期的行為越俎代庖
        _mockRepository.VerifyAll();
    }
}
    