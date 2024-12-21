using System.Collections.Concurrent;
using System.Text.Json.Serialization;

namespace Bliztard.Application.Model;

public class MachineInfo
{
    [JsonPropertyName("id")]
    public Guid            Id       { init; get; } 

    [JsonPropertyName("type")]
    public MachineType     Type     { init; get; }

    [JsonPropertyName("resource")]
    public MachineResource Resource { init; get; } = new();

    [JsonPropertyName("alive")]
    public bool            Alive    { set;  get; }
}

public enum MachineType
{
    Master,
    Slave
}

public class MachineResource
{
    [JsonPropertyName("baseUrl")]
    public string BaseUrl { set; get; } = "";
}
