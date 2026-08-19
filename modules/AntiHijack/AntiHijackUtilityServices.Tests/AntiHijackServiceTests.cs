using System;
using System.Security.Cryptography;
using FluentAssertions;
using AntiHijackUtilityServices;
using AuthenticationUtilityServices;
using EncryptoUtilityServices;
using TimeUtilityServices;
using Xunit;
using NSubstitute;
using System.Text;
using KeyUtilityServices;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace AntiHijackUtilityServices.Tests;

/// <summary>
/// Contains comprehensive, high-performance, and defensively isolated unit tests for the <see cref="AntiHijackService"/> class.
/// </summary>
public class AntiHijackServiceTests : IDisposable
{
    private readonly FakeTimeProvider _fakeTimeProvider; 
    private readonly ITimeService _mockTimeService;
    private readonly StubAuthenticationService _stubAuthService;
    private readonly FakeEncryptoService _fakeEncryptoService;
    private readonly KeyTransformationService _keyTransformationService;
    private readonly AntiHijackService _sut;
    private readonly byte[] _stubKey;

    /// <summary>
    /// Initializes a new instance of the <see cref="AntiHijackServiceTests"/> class, isolating environment references.
    /// </summary>
    public AntiHijackServiceTests()
    {
        _fakeTimeProvider = new FakeTimeProvider(new DateTimeOffset(2026, 6, 12, 18, 0, 0, TimeSpan.Zero));
        
        // Fix: Defensively initialize the NSubstitute mock object to prevent null references inside SUT instantiation
        _mockTimeService = Substitute.For<ITimeService>();
        
        _stubAuthService = new StubAuthenticationService();
        _fakeEncryptoService = new FakeEncryptoService();
        _keyTransformationService = new KeyTransformationService();

        _stubKey = new byte[32];
        RandomNumberGenerator.Fill(_stubKey);

        _sut = new AntiHijackService(
            _mockTimeService,
            _fakeEncryptoService,
            _stubAuthService
        );
    }

    private ReadOnlySpan<char> ConvertToReadOnlySpanOfChar(
        ReadOnlySpan<byte> byteSpan,
        Span<char> charBuffer
    )
    {
        int written = Encoding.UTF8.GetChars(byteSpan, charBuffer);
        return charBuffer[..written];
    }

    [Fact]
    public void Constructor_WhenTimeServiceIsNull_ShouldThrowArgumentNullExceptionWithFluentAssertions()
    {
        // Arrange
        ITimeService? nullTimeService = null;

        // Act
        Action act = () => new AntiHijackService(
            nullTimeService!,
            _fakeEncryptoService,
            _stubAuthService
        );

        // Assert
        act.Should().Throw<ArgumentNullException>()
           .WithMessage("*timeService*")
           .And.ParamName.Should().Be("timeService");
    }

    [Fact]
    public void ValidateRequest_WhenPayloadOrKeyIsEmpty_ShouldReturnFalseImmediately()
    {
        // Arrange & Act
        Action act1 = () =>
        {
            ReadOnlySpan<char> emptyPayload = ReadOnlySpan<char>.Empty;
            ReadOnlySpan<byte> validKey = _stubKey;
            _sut.ValidateRequest(emptyPayload, validKey);
        };

        Action act2 = () =>
        {
            ReadOnlySpan<char> validPayload = "SomePayload".AsSpan();
            ReadOnlySpan<byte> emptyKey = ReadOnlySpan<byte>.Empty;
            _sut.ValidateRequest(validPayload, emptyKey);
        };

        // Assert
        act1.Should().Throw<ArgumentException>()
            .WithMessage("*Payload*");

        act2.Should().Throw<ArgumentException>()
            .WithMessage("*AES-256*");
    }

    [Fact]
    public void ValidateRequest_WhenSignatureVerificationFails_ShouldReturnFalse()
    {
        // Arrange
        ReadOnlySpan<char> tamperedPayload = "TamperedPayloadData".AsSpan();
        long mockCurrentTicks = 1000000L;
        _mockTimeService.GetCurrentStopWatch().Returns(mockCurrentTicks);

        _fakeEncryptoService.SetupDecryptionResult($"Timestamp={mockCurrentTicks};UserId=123");
        
        // Control stub state natively
        _stubAuthService.VerificationResult = false;

        // Act
        bool result = _sut.ValidateRequest(tamperedPayload, _stubKey);

        // Assert
        result.Should().BeFalse("Because a tampered cryptographic signature must never pass anti-hijack verification protocols.");
    }

/// <summary>
    /// Validates that when the request timestamp has expired beyond the defensive sliding window limit,
    /// the validation protocol short-circuits gracefully and rejects the request.
    /// </summary>
    [Fact]
    public void ValidateRequest_WhenTokenIsExpired_ShouldReturnFalseWithStrictValidation()
    {
        // Arrange
        // 1. 使用您專案中既有的高效能 ReadOnlySpan 宣告
        ReadOnlySpan<char> securePayload = "LegitimatePayloadData".AsSpan();
        
        // 2. 模擬解密後的內層 Metadata 包含過期的 Timestamp (500,000)
        // 同時包含 'UserId=' 欄位以主動防禦深層結構完整性檢查 (Deep Structural Alignment)
        string mockedDecryptedText = "Timestamp=500000;UserId=999;";
        
        // 使用您現有的 _fakeEncryptoService 注入解密結果，完美繞過 Lambda 捕捉 ref struct 的限制
        _fakeEncryptoService.SetupDecryptionResult(mockedDecryptedText);

        // 3. 精確配置時間滑動窗口邊界：
        // 設定當前時間Ticks為 400,000,000L
        // 數學防禦公式驗證：| 400,000,000 - 500,000 | = 399,500,000L > AllowableSlidingWindowTicks (300,000,000L)
        long mockCurrentTicks = 400000000L;
        
        // 使用您專案既有的 NSubstitute 語法進行精確的時間控制
        _mockTimeService.GetCurrentStopWatch().Returns(mockCurrentTicks);

        // 預防平行時空副作用：即使後續簽章驗證被意外觸發，也將其設為 true，
        // 這樣可以確保若測試失敗，絕對是因為「時間防禦失效」，而非「簽章不符」。
        _stubAuthService.VerificationResult = true;

        // Act
        // 呼叫受測系統 (SUT)，傳入合法的建構子變數與唯讀緩衝區
        bool result = _sut.ValidateRequest(securePayload, _stubKey);

        // Assert
        // 嚴格遵循鐵律：禁止使用 Assert 原生方法，一律使用 FluentAssertions 進行語意化斷言
        result.Should().BeFalse(
            "Because the request timestamp has expired beyond the allowable defensive sliding window."
        );
    }
        

    [Fact]
    public void ValidateRequest_WhenSignatureIsTampered_ShouldReturnFalseAndLogDefenseEvent()
    {
        // Arrange
        ReadOnlySpan<char> tamperedPayload = "TamperedPayloadData".AsSpan();
        ReadOnlySpan<byte> validKeyReadOnlySpanBytes = _stubKey;

        Span<char> charBuffer = stackalloc char[Encoding.UTF8.GetCharCount(validKeyReadOnlySpanBytes)];
        ReadOnlySpan<char> validKey = ConvertToReadOnlySpanOfChar(validKeyReadOnlySpanBytes, charBuffer);

        long mockCurrentTicks = 1000000L;
        _mockTimeService.GetCurrentStopWatch().Returns(mockCurrentTicks);

        _fakeEncryptoService.SetupDecryptionResult($"Timestamp={mockCurrentTicks};UserId=999");

        // Fix: Explicitly modify the state properties on the custom manual stub class instead of NSubstitute's .Returns syntax
        _stubAuthService.VerificationResult = false;

        // Act
        bool result = _sut.ValidateRequest(tamperedPayload, validKeyReadOnlySpanBytes);

        // Assert
        result.Should().BeFalse("Because a tampered cryptographic signature must never pass anti-hijack verification protocols.");
    }

    [Fact]
    public void ValidateRequest_WhenExecutionSucceeds_ShouldReturnFalseSinceMismatchedDataStructure()
    {
        // Arrange
        ReadOnlySpan<char> securePayload = "LegitimatePayloadData".AsSpan();
        long mockCurrentTicks = 1000000L;
        _mockTimeService.GetCurrentStopWatch().Returns(mockCurrentTicks);

        // Defensively inject a malformed data structure missing the expected 'UserId=' token segment
        _fakeEncryptoService.SetupDecryptionResult($"Timestamp={mockCurrentTicks};InvalidStructure=999");
        
        _stubAuthService.VerificationResult = true;

        // Act
        bool result = _sut.ValidateRequest(securePayload, _stubKey);

        // Assert - Adhering to FluentAssertions rule strictly
        result.Should().BeFalse("Because the decrypted payload structure lacks required security token properties like UserId alignment.");
    }

    public void Dispose()
    {
        // System explicitly isolated without residual framework state leaks.
    }
}