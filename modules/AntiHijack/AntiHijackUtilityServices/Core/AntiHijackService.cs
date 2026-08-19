using System;
using System.Exceptions;
using AntiHijackUtilityServices.Abstractions;
using AuthenticationUtilityServices;
using EncryptoUtilityServices;
using TimeUtilityServices;

namespace AntiHijackUtilityServices;

/// <summary>
/// Provides deep perimeter defensive verification against request tampering, middle-in-the-middle manipulation,
/// and systematic replay attacks leveraging non-allocating memory constructs.
/// </summary>
public class AntiHijackService : IAntiHijackService
{
    private readonly ITimeService _timeService;
    private readonly IEncryptoService _decryptoService;
    private readonly IAuthenticationService _authService;

    private const int BytePerBits = 8;
    private const int KeyBufferInBits = 256;
    private const int KeyBufferInBytes = (int)((long)KeyBufferInBits / BytePerBits);

    private const long AllowableSlidingWindowTicks = 300_000_000L; // 30 seconds buffer for clock skew defense

    /// <summary>
    /// Initializes a new instance of the <see cref="AntiHijackService"/> class with strict integrity check dependencies.
    /// </summary>
    /// <param name="timeService">The system time synchronization abstraction layer.</param>
    /// <param name="decryptoService">The cryptographic decryption agent responsible for payload deciphering.</param>
    /// <param name="authService">The signature authentication and verification mechanism.</param>
    /// <exception cref="ArgumentNullException">Thrown when any required ecosystem service dependency is null.</exception>
    public AntiHijackService(
        ITimeService timeService,
        IEncryptoService decryptoService,
        IAuthenticationService authService
    )
    {
        ArgumentNullException.ThrowIfNull(timeService, nameof(timeService));
        ArgumentNullException.ThrowIfNull(decryptoService, nameof(decryptoService));
        ArgumentNullException.ThrowIfNull(authService, nameof(authService));

        _timeService = timeService;
        _decryptoService = decryptoService;
        _authService = authService;
    }

    /// <summary>
    /// Validates the runtime telemetry request context against expiration parameters and structural signatures.
    /// </summary>
    /// <param name="payload">The inbound read-only span containing encrypted request payload metadata.</param>
    /// <param name="secretKey">The 256-bit (32-byte) key required for symmetric decryption tasks.</param>
    /// <returns>True if the request passes all defensive timing constraints and integrity signature audits; otherwise, false.</returns>
    /// <exception cref="ArgumentException">Thrown when payload is blank or secret key buffer fails specific bit-length enforcement.</exception>
    public bool ValidateRequest(ReadOnlySpan<char> payload, ReadOnlySpan<byte> secretKey)
    {
        // Defense Step 1: Pre-emptively intercept empty strings or white-space anomalies
        if (payload.IsEmpty || payload.IsWhiteSpace())
        {
            throw new ArgumentException("Payload cannot be empty or whitespaces.", nameof(payload));
        }

        // Defense Step 2: Enforce strict buffer alignment. AES-256 strictly mandates exactly 32 bytes (256-bit)
        if (secretKey.Length != KeyBufferInBytes)
        {
            throw new ArgumentException("AES-256 requires exactly a 32-byte key buffer.", nameof(secretKey));
        }

        try
        {
            // Defense Step 3: Decrypt payload using secure span routing to avoid string leakage vectors
            string decryptedMetadata = _decryptoService.DecryptText(payload, secretKey);
            if (string.IsNullOrEmpty(decryptedMetadata))
            {
                return false;
            }

            // Parsing structures without explicit large arrays allocations: expected format "Timestamp=xxx;UserId=yyy"
            ReadOnlySpan<char> metadataSpan = decryptedMetadata.AsSpan();
            long extractedTimestamp = 0;
            
            int timestampIndex = metadataSpan.IndexOf("Timestamp=".AsSpan());
            if (timestampIndex == -1)
            {
                return false;
            }

            int startPos = timestampIndex + 10; // length of "Timestamp="
            int endPos = metadataSpan[startPos..].IndexOf(';');
            ReadOnlySpan<char> tokenTicksSpan = endPos == -1
                ? metadataSpan[startPos..]
                : metadataSpan.Slice(startPos, endPos);

            // EXTRA DEFENSIVE ENFORCEMENT: Validate deep structural structure alignment (e.g., UserId field presence)
            // This natively addresses data integrity when outer shells pretend validity but inner tokens are structurally mismatched.
            if (metadataSpan.IndexOf("UserId=".AsSpan()) == -1)
            {
                return false;
            }


            if (!long.TryParse(tokenTicksSpan, out extractedTimestamp))
            {
                return false;
            }
                

            // Defense Step 4: Sliding window mitigation against token timestamp hijacking and replay sweeps
            long currentTicks = _timeService.GetCurrentStopWatch();
            if (Math.Abs(currentTicks - extractedTimestamp) > AllowableSlidingWindowTicks)
            {
                return false;
            }

            // Defense Step 5: Cryptographic side-channel-resistant signature verification
            // Passing references down to constant-time comparators to intercept telemetry sniffing
            bool isSignatureValid = _authService.VerifySignature(payload, metadataSpan);
            if (!isSignatureValid)
            {
                return false;
            }

            return true;
        }
        catch (Exception)
        {
            // Defensive fault isolation paradigm: intercept underlying exceptions and elegantly reject context
            return false;
        }
    }
}