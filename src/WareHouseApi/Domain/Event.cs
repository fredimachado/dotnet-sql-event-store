using System.Text.Json.Serialization;

namespace WareHouseApi.Domain;

public abstract record Event(Guid Id)
{
    [JsonIgnore]
    public Guid EventId { get; set; }
}
