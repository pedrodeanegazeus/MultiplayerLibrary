using System.Text;

namespace MultiplayerLibrary.Extensions;

public static class ByteArrayExtensions
{
    public static short ToShort(this byte[] value)
    {
        if (value.Length != 2)
        {
            throw new ArgumentException("ByteArrayExtensions.ToShort needs 2 bytes");
        }
        short @short = (short)((value[0] << 8) + value[1]);
        return @short;
    }

    public static string ToUTF8String(this byte[] value)
    {
        string @string = Encoding.UTF8.GetString(value);
        return @string;
    }
}
