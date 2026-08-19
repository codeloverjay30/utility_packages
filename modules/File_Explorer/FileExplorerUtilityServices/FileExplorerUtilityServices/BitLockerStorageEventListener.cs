using System;
using System.Management;
using EnvironmentUtilityServices;

namespace FileExplorerUtilityServices;

/// <summary>
/// Listens to Windows Management Instrumentation (WMI) volume events to detect BitLocker unlock states 
/// and orchestrates shell refresh notifications defensively.
/// </summary>
public class BitLockerStorageEventListener : IBitLockerStorageEventListener, IDisposable
{
    private readonly IBitLockerShellRefresher _shellRefresher;
    private readonly IEnvironmentService _environmentService;
    private ManagementEventWatcher? _watcher;
    private bool _isDisposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="BitLockerStorageEventListener"/> class.
    /// </summary>
    /// <param name="shellRefresher">The shell refresher service to trigger Explorer icon updates.</param>
    /// <param name="environmentService">The environment service to determine OS capabilities.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="shellRefresher"/> or <paramref name="environmentService"/> is null.</exception>
    public BitLockerStorageEventListener(
        IBitLockerShellRefresher shellRefresher,
        IEnvironmentService environmentService)
    {
        ArgumentNullException.ThrowIfNull(shellRefresher, nameof(shellRefresher));
        ArgumentNullException.ThrowIfNull(environmentService, nameof(environmentService));

        _shellRefresher = shellRefresher;
        _environmentService = environmentService;
    }

    /// <summary>
    /// Starts listening to __InstanceModificationEvent for Win32_Volume instances.
    /// </summary>
    public void StartListening()
    {
        EnsureWindowsPlatform();

        if (_watcher != null)
        {
            return; // Already listening, prevent duplicate tracking.
        }

        // Defensive WMI Polling Query: Monitor drive unlock state transitions safely every 2 seconds.
        string query = "SELECT * FROM __InstanceModificationEvent WITHIN 2 WHERE TargetInstance ISA 'Win32_Volume'";
        
        _watcher = new ManagementEventWatcher(query);
        _watcher.EventArrived += OnVolumeChanged;
        _watcher.Start();
    }

    /// <summary>
    /// Stops listening to volume events and tears down WMI infrastructure.
    /// </summary>
    public void StopListening()
    {
        if (_watcher != null)
        {
            try
            {
                _watcher.Stop();
                _watcher.EventArrived -= OnVolumeChanged;
            }
            catch (Exception)
            {
                // Defensive recovery: Suppress low-level OS infrastructure tracking exceptions during teardown.
            }
            finally
            {
                _watcher.Dispose();
                _watcher = null;
            }
        }
    }

    /// <summary>
    /// Core event callback when WMI detects a properties modification in Win32_Volume.
    /// </summary>
    private void OnVolumeChanged(object sender, EventArrivedEventArgs e)
    {
        try
        {
            if (!_environmentService.IsWindows())
            {
                return; // Asynchronous callback defensive exit.
            }

            using var targetInstance = (ManagementBaseObject)e.NewEvent["TargetInstance"];
            string? driveLetter = targetInstance["DriveLetter"]?.ToString();

            if (!string.IsNullOrEmpty(driveLetter))
            {
                // Memory efficient slicing without GC allocations.
                ReadOnlySpan<char> driveSpan = driveLetter.AsSpan();
                _shellRefresher.NotifyToRefresh(driveSpan);
            }
        }
        catch (Exception)
        {
            // Defensive architecture guard: Never allow ambient OS event thread exceptions 
            // to leak into the main calling application pool, preventing silent crashes.
        }
    }

    /// <summary>
    /// Assures that the underlying execution engine resides on Windows OS boundaries.
    /// </summary>
    private void EnsureWindowsPlatform()
    {
        if (!_environmentService.IsWindows())
        {
            throw new PlatformNotSupportedException("This API is only available for Windows");
        }
    }

    /// <summary>
    /// Releases all managed resources used by the listener.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                StopListening();
            }
            _isDisposed = true;
        }
    }
}