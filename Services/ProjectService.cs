using System.Net.Http.Json;
using System.Text.Json;
using FoxbyteLabs.Models;

namespace FoxbyteLabs.Services;

public sealed class ProjectService
{
    public const string DenHomeUrl = "https://den.foxbytelabs.co.za/";
    public const string DenManifestUrl = "https://den.foxbytelabs.co.za/data/apps-manifest.json";
    public const string FfsUrl = "https://ffs.foxbytelabs.co.za/";
    public const string CookbookUrl = "https://hcbw.foxbytelabs.co.za/";
    public const string GitHubProfileUrl = "https://github.com/Marcell0805";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _http;
    private IReadOnlyList<LabProject>? _cache;
    private IReadOnlyList<LabDestination>? _destinations;
    private string _source = "none";

    public ProjectService(HttpClient http)
    {
        _http = http;
    }

    public string Source => _source;

    public async Task<IReadOnlyList<LabProject>> GetFeaturedProjectsAsync(int take = 6, CancellationToken ct = default)
    {
        var all = await GetProjectsAsync(ct);
        return all
            .Where(p => p.Featured)
            .OrderByDescending(p => p.IsActive)
            .ThenByDescending(p => p.Updated)
            .ThenBy(p => p.Name)
            .Take(take)
            .ToList();
    }

    public async Task<IReadOnlyList<LabDestination>> GetDestinationsAsync(CancellationToken ct = default)
    {
        if (_destinations is not null)
            return _destinations;

        try
        {
            var destinations = await _http.GetFromJsonAsync<List<LabDestination>>("data/destinations.json", JsonOptions, ct);
            _destinations = destinations ?? [];
        }
        catch
        {
            _destinations = [];
        }

        return _destinations;
    }

    public async Task<IReadOnlyList<LabProject>> GetProjectsAsync(CancellationToken ct = default)
    {
        if (_cache is not null)
        {
            return _cache;
        }

        var fromDen = await TryLoadFromDenAsync(ct);
        if (fromDen is { Count: > 0 })
        {
            _cache = fromDen;
            _source = "den";
            return _cache;
        }

        var fallback = await TryLoadLocalFallbackAsync(ct);
        _cache = fallback;
        _source = fallback.Count > 0 ? "local" : "empty";
        return _cache;
    }

    private async Task<IReadOnlyList<LabProject>> TryLoadFromDenAsync(CancellationToken ct)
    {
        try
        {
            using var response = await _http.GetAsync(DenManifestUrl, ct);
            if (!response.IsSuccessStatusCode)
            {
                return [];
            }

            var manifest = await response.Content.ReadFromJsonAsync<DenAppsManifest>(JsonOptions, ct);
            if (manifest?.Apps is not { Count: > 0 })
            {
                return [];
            }

            return manifest.Apps
                .Where(a => a.Visible && !string.IsNullOrWhiteSpace(a.Title))
                .Select(MapDenApp)
                .ToList();
        }
        catch
        {
            return [];
        }
    }

    private async Task<IReadOnlyList<LabProject>> TryLoadLocalFallbackAsync(CancellationToken ct)
    {
        try
        {
            var projects = await _http.GetFromJsonAsync<List<LabProject>>("data/projects.json", JsonOptions, ct);
            return projects ?? [];
        }
        catch
        {
            return [];
        }
    }

    private static LabProject MapDenApp(DenAppEntry app)
    {
        DateOnly? updated = null;
        if (!string.IsNullOrWhiteSpace(app.PublishedAt) &&
            DateOnly.TryParse(app.PublishedAt, out var parsed))
        {
            updated = parsed;
        }

        var description = FirstNonEmpty(app.SummaryOverride, app.Note)
                          ?? "A Foxbyte Labs project.";

        // Prefer a short single-line description on cards.
        description = description.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)[0];
        if (description.Length > 160)
        {
            description = description[..157].TrimEnd() + "…";
        }

        return new LabProject
        {
            Id = app.Id,
            Name = app.Title,
            Description = description,
            Status = NormalizeStatus(app.Status),
            Url = ResolveUrl(app),
            Updated = updated,
            Featured = true,
            Category = CategoryFromKind(app.Kind)
        };
    }

    private static string NormalizeStatus(string? status) => status?.Trim().ToLowerInvariant() switch
    {
        "live" => "Live",
        "beta" => "Beta",
        "in development" or "in-development" or "in progress" or "in-progress" or "wip" => "In Progress",
        "experiment" or "experimental" => "Experiment",
        _ => string.IsNullOrWhiteSpace(status) ? "Live" : status
    };

    private static string CategoryFromKind(string? kind) => kind?.Trim().ToLowerInvariant() switch
    {
        "mobile" => "Mobile App",
        "website" or "web" => "Web App",
        "tool" => "Utility",
        _ => "Project"
    };

    private static string ResolveUrl(DenAppEntry app)
    {
        if (app.Id.Equals("ffs", StringComparison.OrdinalIgnoreCase))
            return FfsUrl;
        if (app.Id.Equals("huntress-cookbook", StringComparison.OrdinalIgnoreCase))
            return CookbookUrl;
        if (!string.IsNullOrWhiteSpace(app.ExternalUrl))
            return app.ExternalUrl;
        return $"{DenHomeUrl}sections/{app.Id}.html";
    }

    private static string? FirstNonEmpty(params string?[] values) =>
        values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));
}
