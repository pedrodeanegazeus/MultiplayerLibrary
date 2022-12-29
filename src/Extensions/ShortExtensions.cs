namespace MultiplayerLibrary.Extensions;

public static class ShortExtensions
{
    public static byte[] ToByteArray(this short value)
    {
        byte[] byteArray = new byte[2];
        byteArray[0] = (byte)(value >> 8);
        byteArray[1] = (byte)value;
        return byteArray;
    }
}
