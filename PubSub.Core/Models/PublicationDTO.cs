using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace PubSub.Core.Models;

public class PublicationDTO
{
    [SetsRequiredMembers]
    public PublicationDTO(JsonElement payload)
    {
        Payload = payload;
    }

    public required JsonElement Payload { get; set; }
}