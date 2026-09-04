using System.Text.Json.Serialization;

namespace FoxbyteLabs.Models;

/// <summary>
/// Subset of The Fox's Den apps-manifest.json used by the homepage.
/// </summary>
public sealed class DenAppsManifest
{
    [JsonPropertyName("apps")]
    public List<DenAppEntry> Apps { get; set; } = [];
}

public sealed class DenAppEntry
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("title")]
    public string Title { get; set; } = "";

    [JsonPropertyName("kind")]
    public string Kind { get; set; } = "";

    [JsonPropertyName("visible")]
    public bool Visible { get; set; } = true;

    [JsonPropertyName("status")]
    public string Status { get; set; } = "live";

    [JsonPropertyName("summaryOverride")]
    public string? SummaryOverride { get; set; }

    [JsonPropertyName("note")]
    public string? Note { get; set; }

    [JsonPropertyName("externalUrl")]
    public string? ExternalUrl { get; set; }

    [JsonPropertyName("publishedAt")]
    public string? PublishedAt { get; set; }

    [JsonPropertyName("tags")]
    public List<string>? Tags { get; set; }
}
