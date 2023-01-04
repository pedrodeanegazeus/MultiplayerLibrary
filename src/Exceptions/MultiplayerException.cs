namespace MultiplayerLibrary.Exceptions;

public class MultiplayerException : Exception
{
    public short Code { get; set; }

    public MultiplayerException((short code, string message) error, params object?[] args)
        : this(null, error, args)
    { }

    public MultiplayerException(Exception? innerException, (short code, string message) error, params object?[] args)
        : base(string.Format(error.message, args), innerException)
    {
        Code = error.code;
    }
}
