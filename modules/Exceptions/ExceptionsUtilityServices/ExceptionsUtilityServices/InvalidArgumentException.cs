namespace ExceptionsUtilityServices;

/// <summary>
/// 
/// </summary>
public class InvalidArgumentException : ArgumentException
{
    public InvalidArgumentException() : 
        base()
    {
        
    }
    public InvalidArgumentException(string message) : 
        base(message)
    {

    }

    public InvalidArgumentException(string message, string paramName) : 
        base(message, paramName)
    {
        
    }

    public InvalidArgumentException(string message, Exception innerException) : 
        base(message, innerException)
    {

    }

    public InvalidArgumentException(string message, string paramName, Exception innerException) : 
        base(message, paramName, innerException)
    {

    }

    public static void ThrowIfNull<T>(
        T instance,
        string message,
        string paramName = "",
        Exception innerException = null
    )
    {
        if (instance == null)
        {
            if (innerException != null)
            {
                if (!string.IsNullOrWhiteSpace(paramName))
                {
                    throw new InvalidArgumentException(message, paramName, innerException);
                }

                throw new InvalidArgumentException(message, innerException);
            }

            if (!string.IsNullOrWhiteSpace(paramName))
            {
                throw new InvalidArgumentException(message, paramName);
            }
            
            throw new InvalidArgumentException(message);
        }
    }
}
