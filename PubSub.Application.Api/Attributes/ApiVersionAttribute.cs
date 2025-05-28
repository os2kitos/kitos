namespace PubSub.Application.Api.Attributes;

public class ApiVersionAttribute : Attribute
{
    public int Version { get; }

    public ApiVersionAttribute(int version)
    {
        Version = version;
    }
}