using System.ComponentModel;

namespace SpanUtilityServices;

public enum StatusInfo
{
    /// <summary>
    /// The <see cref="global::System.Int32"/> that is converted from <see langword="false"/>. 
    /// </summary>

    [Description("The instance of type (e.g. `Span<T>`) is NOT empty")]
    IsNotEmpty = 1,

    /// <summary>
    /// The <see cref="global::System.Int32"/> that is converted from <see langword="true"/>. 
    /// </summary>
    [Description("The instance of type (e.g. `Span<T>`) is NOT empty")]
    IsEmpty = 0,

    /// <summary>
    /// Has not checked the instance is empty or not. 
    /// </summary>
    [Description("Unknown to check an instance of type (e.g. `Span<T>`) is empty or NOT")]
    Unknown = -1,

    /// <summary>
    /// Unabled to check the instance is empty or not. 
    /// </summary>
    [Description("Unknown to check an instance of type (e.g. `Span<T>`) is empty or NOT due to an exception is thrown during accessing its pointer or address.")]
    FailureTest = -2,
}
