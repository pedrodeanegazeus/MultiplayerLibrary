using System.Reflection;
using MultiplayerLibrary.Attributes;
using MultiplayerLibrary.Extensions;

namespace MultiplayerLibrary.Models.Packages;

public enum PackageType : short
{
    // 0 - Service packages
    Error = 0000,
    Handshake = 0001,
    Ping = 0002,

    // 1 - Communication packages
    JoinChannel = 1000,
    JoinChannelResponse = 1001,
    LeaveChannel = 1002,
    ListChannel = 1003,
    ListChannelResponse = 1004,
    Message = 1005,

    // 2 - Authentication packages
    Authenticate = 2000,
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

    private static async Task AssembleShort(MemoryStream packageStream, List<object> arguments)
    {
        byte[] shortBytes = new byte[2];
        await packageStream.ReadExactlyAsync(shortBytes);
        short shortValue = shortBytes.ToShort();
        arguments.Add(shortValue);
    }

    private static async Task AssembleString(MemoryStream packageStream, List<object> arguments)
    {
        byte[] stringValueSize = new byte[2];
        await packageStream.ReadExactlyAsync(stringValueSize);
        byte[] stringValueBytes = new byte[stringValueSize.ToShort()];
        await packageStream.ReadExactlyAsync(stringValueBytes);
        string stringValue = stringValueBytes.ToUTF8String();
        arguments.Add(stringValue);
    }
}
