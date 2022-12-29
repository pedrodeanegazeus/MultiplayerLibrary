using System.Reflection;
using MultiplayerLibrary.Attributes;
using MultiplayerLibrary.Extensions;
using MultiplayerLibrary.Interfaces.Models;

namespace MultiplayerLibrary.Models.Packages;

public enum PackageType : short
{
    // 0 - Service packages
    Ping = 0000,
    Handshake = 0001,

    // 1 - Communication packages
    Message = 1000,
    JoinChannel = 1001,
    JoinChannelResponse = 1002,
    LeaveChannel = 1003,
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
        IOrderedEnumerable<PropertyInfo> orderedProperties = properties
            .Where(property => Attribute.IsDefined(property, typeof(OrderAttribute)))
            .OrderBy(property => property.GetCustomAttribute<OrderAttribute>()?.Order);
        foreach (PropertyInfo property in orderedProperties)
        {
            switch (property.PropertyType.Name)
            {
                case "Guid":
                    Guid guidValue = (Guid?)property.GetValue(this) ?? Guid.Empty;
                    byte[] guidValueBytes = guidValue.ToByteArray();
                    await packageStream.WriteAsync(guidValueBytes);
                    break;
                case "String":
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
        IOrderedEnumerable<PropertyInfo> orderedProperties = properties
            .Where(property => Attribute.IsDefined(property, typeof(OrderAttribute)))
            .OrderBy(property => property.GetCustomAttribute<OrderAttribute>()?.Order);
        foreach (PropertyInfo property in orderedProperties)
        {
            switch (property.PropertyType.Name)
            {
                case "Guid":
                    byte[] guidBytes = new byte[16];
                    await packageStream.ReadExactlyAsync(guidBytes);
                    Guid guidValue = new(guidBytes);
                    arguments.Add(guidValue);
                    break;
                case "String":
                    byte[] stringValueSize = new byte[2];
                    await packageStream.ReadExactlyAsync(stringValueSize);
                    byte[] stringValueBytes = new byte[stringValueSize.ToShort()];
                    await packageStream.ReadExactlyAsync(stringValueBytes);
                    string stringValue = stringValueBytes.ToUTF8String();
                    arguments.Add(stringValue);
                    break;
                default:
                    throw new NotImplementedException($"Type {property.PropertyType.Name} not implemented");
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
