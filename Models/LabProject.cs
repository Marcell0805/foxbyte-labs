namespace FoxbyteLabs.Models;

public sealed class LabProject
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Status { get; set; } = "Live";
    public string? Url { get; set; }
    public DateOnly? Updated { get; set; }
    public bool Featured { get; set; } = true;
    public string Category { get; set; } = "Project";
    public bool IsActive => Status is "Beta" or "In Progress" or "Experiment";
}
