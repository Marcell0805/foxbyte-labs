using FoxbyteLabs.Models;

namespace FoxbyteLabs.Services;

public static class LabSearch
{
    public static IReadOnlyList<LabSearchHit> Build(IEnumerable<LabDestination> destinations, IEnumerable<LabProject> projects)
    {
        var hits = new List<LabSearchHit>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var destination in destinations)
        {
            if (string.IsNullOrWhiteSpace(destination.Url))
                continue;

            hits.Add(new LabSearchHit
            {
                Id = destination.Id,
                Title = destination.Title,
                Detail = destination.Detail,
                Url = destination.Url,
                Kind = string.IsNullOrWhiteSpace(destination.Eyebrow) ? "Destination" : destination.Eyebrow
            });
            seen.Add(destination.Id);
        }

        foreach (var project in projects)
        {
            if (string.IsNullOrWhiteSpace(project.Url) || seen.Contains(project.Id))
                continue;

            hits.Add(new LabSearchHit
            {
                Id = project.Id,
                Title = project.Name,
                Detail = project.Description,
                Url = project.Url,
                Kind = project.Category
            });
        }

        return hits;
    }

    public static bool Matches(LabSearchHit hit, string query)
    {
        if (Contains(hit.Title, query) || Contains(hit.Detail, query) || Contains(hit.Kind, query) || Contains(hit.Id, query))
            return true;

        var compactQuery = Compact(query);
        return compactQuery.Length > 0 &&
               (Compact(hit.Title).Contains(compactQuery, StringComparison.Ordinal) ||
                Compact(hit.Id).Contains(compactQuery, StringComparison.Ordinal));
    }

    private static bool Contains(string? value, string query) =>
        !string.IsNullOrWhiteSpace(value) &&
        value.Contains(query, StringComparison.OrdinalIgnoreCase);

    private static string Compact(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "";

        Span<char> buffer = stackalloc char[value.Length];
        var count = 0;
        foreach (var ch in value)
        {
            if (char.IsLetterOrDigit(ch))
                buffer[count++] = char.ToLowerInvariant(ch);
        }

        return new string(buffer[..count]);
    }
}
