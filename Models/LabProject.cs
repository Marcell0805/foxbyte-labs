namespace FoxbyteLabs.Models;

public sealed record LabProject(
    string Name,
    string Status,
    string Description,
    string? Url = null);
