using System.Text;

namespace MultiplayerLibrary.Extensions;

public static class StringExtensions
{
    public static byte[] ToUTF8ByteArray(this string value)
    {
        byte[] byteArray = Encoding.UTF8.GetBytes(value);
        return byteArray;
    }
}
