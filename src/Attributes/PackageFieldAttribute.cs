namespace MultiplayerLibrary.Attributes;

public enum FieldType
{
    PlayerInfoArray,
    Guid,
    GuidArray,
    Short,
    String,
}

[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class PackageFieldAttribute : Attribute
{
    public int Order { get; }
    public FieldType Type { get; }

    public PackageFieldAttribute(int order, FieldType type)
    {
        Order = order;
        Type = type;
    }
}
