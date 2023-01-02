using System.Reflection;
using MultiplayerLibrary.Attributes;
using MultiplayerLibrary.Extensions;

namespace MultiplayerLibrary.Models.Packages;

public enum PackageType : short
{
    // 0 - Service packages
    Error = 0000,
    Ping = 0001,
    Handshake = 0002,

    // 1 - Communication packages
    Message = 1000,
    JoinChannel = 1001,
    JoinChannelResponse = 1002,
    LeaveChannel = 1003,
    ListChannels = 1004,
    ListChannelsResponse = 1005,
}

public abstract class Package
{
    public abstract PackageType Type { get; }
    public abstract byte Version { get; }

    public async Task<byte[]> ToByteArrayAsync()
    {
        using MemoryStream packageStream = new();
        await packageStream.WriteAsync(((short)Type).ToByteArray());
        packageStream.WriteByte(Version);
        Type type = GetType();
        PropertyInfo[] properties = type.GetProperties();
        IOrderedEnumerable<(PackageFieldAttribute? attribute, PropertyInfo property)> orderedProperties = properties
            .Where(property => Attribute.IsDefined(property, typeof(PackageFieldAttribute)))
            .Select(property => (attribute: property.GetCustomAttribute<PackageFieldAttribute>(), property))
            .OrderBy(property => property.attribute?.Order);
        foreach ((PackageFieldAttribute? attribute, PropertyInfo property) in orderedProperties)
        {
            switch (attribute?.Type)
            {
                case FieldType.Channels:
                    List<(Guid, string, short)> channels = (List<(Guid, string, short)>?)property.GetValue(this) ?? new List<(Guid, string, short)>();
                    await packageStream.WriteAsync(((short)channels.Count).ToByteArray());
                    foreach ((Guid channelId, string channelName, short connectionsCount) in channels)
                    {
                        byte[] channelIdValueBytes = channelId.ToByteArray();
                        await packageStream.WriteAsync(channelIdValueBytes);
                        byte[] channelNameValueBytes = channelName.ToUTF8ByteArray();
                        await packageStream.WriteAsync(((short)channelNameValueBytes.Length).ToByteArray());
                        await packageStream.WriteAsync(channelNameValueBytes);
                        await packageStream.WriteAsync((connectionsCount).ToByteArray());
                    }
                    break;
                case FieldType.Guid:
                    Guid guidValue = (Guid?)property.GetValue(this) ?? Guid.Empty;
                    byte[] guidValueBytes = guidValue.ToByteArray();
                    await packageStream.WriteAsync(guidValueBytes);
                    break;
                case FieldType.Short:
                    short shortValue = (short?)property.GetValue(this) ?? 0;
                    byte[] shortValueBytes = shortValue.ToByteArray();
                    await packageStream.WriteAsync(shortValueBytes);
                    break;
                case FieldType.String:
                    string stringValue = (string?)property.GetValue(this) ?? string.Empty;
                    byte[] stringValueBytes = stringValue.ToUTF8ByteArray();
                    await packageStream.WriteAsync(((short)stringValueBytes.Length).ToByteArray());
                    await packageStream.WriteAsync(stringValueBytes);
                    break;
                default:
                    throw new NotImplementedException($"Type {property.PropertyType.Name} not implemented");
            }
        }
        byte[] packageBytes = packageStream.ToArray();
        return packageBytes;
    }

    public static async Task<TPackage> CreateAsync<TPackage>(byte[] bytes)
        where TPackage : Package
    {
        using MemoryStream packageStream = new(bytes);
        byte[] header = new byte[3]; // package type (2 bytes) + version (1 byte)
        await packageStream.ReadExactlyAsync(header);
        List<object> arguments = new();
        Type type = typeof(TPackage);
        PropertyInfo[] properties = type.GetProperties();
        IOrderedEnumerable<PackageFieldAttribute?> orderedAttributes = properties
            .Where(property => Attribute.IsDefined(property, typeof(PackageFieldAttribute)))
            .Select(property => property.GetCustomAttribute<PackageFieldAttribute>())
            .OrderBy(property => property?.Order);
        foreach (PackageFieldAttribute? attribute in orderedAttributes)
        {
            switch (attribute?.Type)
            {
                case FieldType.Channels:
                    byte[] channelListSizeBytes = new byte[2];
                    await packageStream.ReadExactlyAsync(channelListSizeBytes);
                    short channelListSize = channelListSizeBytes.ToShort();
                    List<(Guid, string, short)> channels = new List<(Guid, string, short)>(channelListSize);
                    for (int it = 0; it < channelListSize; it++)
                    {
                        byte[] channelIdBytes = new byte[16];
                        await packageStream.ReadExactlyAsync(channelIdBytes);
                        Guid channelId = new(channelIdBytes);
                        byte[] channelNameSize = new byte[2];
                        await packageStream.ReadExactlyAsync(channelNameSize);
                        byte[] channelNameBytes = new byte[channelNameSize.ToShort()];
                        await packageStream.ReadExactlyAsync(channelNameBytes);
                        string channelName = channelNameBytes.ToUTF8String();
                        byte[] connectionsCountBytes = new byte[2];
                        await packageStream.ReadExactlyAsync(connectionsCountBytes);
                        short connectionsCount = connectionsCountBytes.ToShort();
                        channels.Add((channelId, channelName, connectionsCount));
                    }
                    arguments.Add(channels);
                    break;
                case FieldType.Guid:
                    byte[] guidBytes = new byte[16];
                    await packageStream.ReadExactlyAsync(guidBytes);
                    Guid guidValue = new(guidBytes);
                    arguments.Add(guidValue);
                    break;
                case FieldType.Short:
                    byte[] shortBytes = new byte[2];
                    await packageStream.ReadExactlyAsync(shortBytes);
                    short shortValue = shortBytes.ToShort();
                    arguments.Add(shortValue);
                    break;
                case FieldType.String:
                    byte[] stringValueSize = new byte[2];
                    await packageStream.ReadExactlyAsync(stringValueSize);
                    byte[] stringValueBytes = new byte[stringValueSize.ToShort()];
                    await packageStream.ReadExactlyAsync(stringValueBytes);
                    string stringValue = stringValueBytes.ToUTF8String();
                    arguments.Add(stringValue);
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
}
