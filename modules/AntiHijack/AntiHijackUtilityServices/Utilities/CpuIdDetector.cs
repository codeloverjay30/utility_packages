using System.Runtime.InteropServices;
using AntiHijackUtilityServices.Abstractions;
using EnvironmentUtilityServices;

namespace AntiHijackUtilityService.Sensors;

/// <summary>
/// Detects presence of a hypervisor using raw CPUID instruction execution.
/// </summary>
public class CpuIdDetector : ISafetySensor
{
    public string SensorName => "CpuIdHypervisorDetector";

    [StructLayout(LayoutKind.Sequential)]
    private struct CpuIdResult
    {
        public uint Eax;
        public uint Ebx;
        public uint Ecx;
        public uint Edx;
    }

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate void CpuIdDelegate(out CpuIdResult result, uint leaf);

    [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
    private static extern bool VirtualProtect(IntPtr lpAddress, UIntPtr dwSize, uint flNewProtect, out uint lpflOldProtect);

    private const uint PAGE_EXECUTE_READWRITE = 0x40;

    private readonly IPlatformService _platformService;
    
    public CpuIdDetector(
        IPlatformService platformService
    )
    {
        ArgumentNullException.ThrowIfNull(platformService, nameof(platformService));
        _platformService = platformService;
    }
    /// <summary>
    /// Executes the raw assembly code via high-performance ReadOnlySpan to check hypervisor bit.
    /// </summary>
    public bool IsThreatDetected()
    {
        if (!_platformService.IsWindows()) 
        {
            throw new PlatformNotSupportedException("CpuIdDetector is only supported on Windows platforms.");
        }

        // Optimized with ReadOnlySpan to ensure non-allocating, immutable bytecode declaration
        ReadOnlySpan<byte> x64Code = [
            0x53, 0x48, 0x89, 0xD0, 0x0F, 0xA2, 0x48, 0x89,
            0x01, 0x89, 0x59, 0x04, 0x89, 0x49, 0x08, 0x89,
            0x51, 0x0C, 0x5B, 0xC3
        ];

        IntPtr memoryPointer = Marshal.AllocHGlobal(x64Code.Length);
        if (memoryPointer == IntPtr.Zero)
        {
            throw new InsufficientMemoryException("Failed to allocate unmanaged memory for CPUID bytecode.");
        }

        uint oldProtect = 0;
        bool isProtectionModified = false;

        try
        {
            unsafe
            {
                // 建立一個大小與機器碼完全一致、直接映射到 Win32 Native Heap 的虛擬寫入視窗
                Span<byte> unmanagedDestination = new Span<byte>(memoryPointer.ToPointer(), x64Code.Length);
                
                // 完美的零配置（Zero Allocation）複製！直接將 DLL 唯讀資料區段的 bytecode 拷貝至 Native 記憶體
                x64Code.CopyTo(unmanagedDestination);
            }
            // Defensively changing the page protection and guaranteeing state rollback via standard Windows API protocols
            if (!VirtualProtect(memoryPointer, (UIntPtr)x64Code.Length, PAGE_EXECUTE_READWRITE, out oldProtect))
            {
                throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error(), "Failed to modify memory protection flags.");
            }
            isProtectionModified = true;

            var cpuIdFunction = Marshal.GetDelegateForFunctionPointer<CpuIdDelegate>(memoryPointer);

            // Standard EAX=1 to get processor info and feature bits
            cpuIdFunction(out CpuIdResult result, 1);

            // Bit 31 of ECX is the hypervisor present bit
            return (result.Ecx & (1U << 31)) != 0;
        }
        finally
        {
            // 安全性優化：將記憶體分頁權限還原至原始狀態，消滅隱患斷面
            if (isProtectionModified)
            {
                _ = VirtualProtect(memoryPointer, (UIntPtr)x64Code.Length, oldProtect, out _);
            }

            if (memoryPointer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(memoryPointer);
            }
        }
    }
}