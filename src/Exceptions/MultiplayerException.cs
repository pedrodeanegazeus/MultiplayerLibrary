namespace MultiplayerLibrary.Exceptions;

public class MultiplayerException : Exception
{
    public short Code { get; set; }

    public MultiplayerException()
        : this(0)
    { }

    public MultiplayerException(short code)
        : this(code, string.Empty)
    { }

    public MultiplayerException(short code, string message)
        : this(code, message, null)
    { }

    public MultiplayerException(short code, string message, Exception? innerException)
        : base(message, innerException)
    {
        Code = code;
    }
}
