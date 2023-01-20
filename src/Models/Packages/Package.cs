using System.Reflection;
using MultiplayerLibrary.Attributes;
using MultiplayerLibrary.Extensions;

namespace MultiplayerLibrary.Models.Packages;

public enum PackageType : ushort
{
    // 0 - Service packages
    Error = 000,
    Handshake = 001,
    Ping = 002,

    // 1 - Communication packages
    JoinChannel = 100,
    JoinChannelResponse = 101,
    LeaveChannel = 102,
    ListChannel = 103,
    ListChannelResponse = 104,
    Message = 105,

    // 2 - Player packages
    Authenticate = 200,
    AuthenticateResponse = 201,
}

public abstract class Package
{
    public static async Task<TPackage> CreateAsync<TPackage>(byte[] bytes)
        where TPackage : Package
    {
        Type type = typeof(TPackage);
        PropertyInfo[] properties = type.GetProperties();
        IOrderedEnumerable<PackageFieldAttribute?> orderedAttributes = properties
            .Where(property => Attribute.IsDefined(property, typeof(PackageFieldAttribute)))
            .Select(property => property.GetCustomAttribute<PackageFieldAttribute>())
            .OrderBy(property => property?.Order);
        List<object> arguments = new();
        using MemoryStream packageStream = new(bytes);
        foreach (PackageFieldAttribute? attribute in orderedAttributes)
        {
            switch (attribute?.Type)
            {
                case FieldType.Guid:
                    await AssembleGuid(packageStream, arguments);
                    break;
                case FieldType.GuidArray:
                    await AssembleGuidArray(packageStream, arguments);
                    break;
                case FieldType.PlayerInfoArray:
                    await AssemblePlayerInfoArray(packageStream, arguments);
                    break;
                case FieldType.Short:
                    await AssembleShort(packageStream, arguments);
                    break;
                case FieldType.String:
                    await AssembleString(packageStream, arguments);
                    break;
                default:
                    throw new NotImplementedException($"Type {attribute?.Type} not implemented");
            }
        }
        TPackage? package = (TPackage?)Activator.CreateInstance(type, arguments.ToArray());
        if (package is null)
        {
            throw new TypeLoadException($"Error creating instance of {type.Name}");
        }
        return package;
    }

    private static async Task AssembleGuid(MemoryStream packageStream, List<object> arguments)
    {
        byte[] guidBytes = new byte[16];
        await packageStream.ReadExactlyAsync(guidBytes);
        Guid guidValue = new(guidBytes);
        arguments.Add(guidValue);
    }

    private static async Task AssembleGuidArray(MemoryStream packageStream, List<object> arguments)
    {
        byte[] guidArrayLengthBytes = new byte[2];
        await packageStream.ReadExactlyAsync(guidArrayLengthBytes);
        short guidArrayLength = guidArrayLengthBytes.ToShort();
        Guid[] guidArray = new Guid[guidArrayLength];
        for (int it = 0; it < guidArrayLength; it++)
        {
            byte[] guidBytes = new byte[16];
            await packageStream.ReadExactlyAsync(guidBytes);
            guidArray[it] = new(guidBytes);
        }
        arguments.Add(guidArray);
    }

    private static async Task AssemblePlayerInfoArray(MemoryStream packageStream, List<object> arguments)
    {
        byte[] connectionArrayLengthBytes = new byte[2];
        await packageStream.ReadExactlyAsync(connectionArrayLengthBytes);
        short connectionArrayLength = connectionArrayLengthBytes.ToShort();
        (Guid connection, string avatarUrl, string displayName)[] connectionArray = new (Guid, string, string)[connectionArrayLength];
        for (int it = 0; it < connectionArrayLength; it++)
        {
            // connection
            byte[] connectionBytes = new byte[16];
            await packageStream.ReadExactlyAsync(connectionBytes);
            connectionArray[it].connection = new(connectionBytes);
            // avatarUrl
            byte[] avatarUrlValueSizeBytes = new byte[2];
            await packageStream.ReadExactlyAsync(avatarUrlValueSizeBytes);
            short avatarUrlValueSize = avatarUrlValueSizeBytes.ToShort();
            byte[] avatarUrlValueBytes = new byte[avatarUrlValueSize];
            await packageStream.ReadExactlyAsync(avatarUrlValueBytes);
            connectionArray[it].avatarUrl = avatarUrlValueBytes.ToUTF8String();
            // displayName
            byte[] displayNameValueSizeBytes = new byte[2];
            await packageStream.ReadExactlyAsync(displayNameValueSizeBytes);
            short displayNameValueSize = displayNameValueSizeBytes.ToShort();
            byte[] displayNameValueBytes = new byte[displayNameValueSize];
            await packageStream.ReadExactlyAsync(displayNameValueBytes);
            connectionArray[it].displayName = displayNameValueBytes.ToUTF8String();
        }
        arguments.Add(connectionArray);
    }

    private static async Task AssembleShort(MemoryStream packageStream, List<object> arguments)
    {
        byte[] shortBytes = new byte[2];
        await packageStream.ReadExactlyAsync(shortBytes);
        short shortValue = shortBytes.ToShort();
        arguments.Add(shortValue);
    }

    private static async Task AssembleString(MemoryStream packageStream, List<object> arguments)
    {
        byte[] stringValueSizeBytes = new byte[2];
        await packageStream.ReadExactlyAsync(stringValueSizeBytes);
        short stringValueSize = stringValueSizeBytes.ToShort();
        byte[] stringValueBytes = new byte[stringValueSize];
        await packageStream.ReadExactlyAsync(stringValueBytes);
        string stringValue = stringValueBytes.ToUTF8String();
        arguments.Add(stringValue);
    }
}
