namespace FoxbyteLabs.Services;

public static class WorkshopClock
{
    public static string Slot(DateTime local) => local.Hour switch
    {
        >= 5 and < 11 => "morning",
        >= 11 and < 17 => "day",
        >= 17 and < 21 => "evening",
        _ => "night"
    };

    public static string Image(string slot) => $"images/workshop/{slot}.png";
}
