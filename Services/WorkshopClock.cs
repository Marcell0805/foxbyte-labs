namespace FoxbyteLabs.Services;

public static class WorkshopClock
{
    public static string Slot(DateTime local)
    {
        var time = TimeOnly.FromDateTime(local);

        if (time >= new TimeOnly(5, 0) && time < new TimeOnly(11, 0))
            return "morning";
        if (time >= new TimeOnly(11, 0) && time < new TimeOnly(16, 30))
            return "day";
        if (time >= new TimeOnly(16, 30) && time < new TimeOnly(18, 59))
            return "evening";

        return "night";
    }

    public static string Image(string slot) => $"images/workshop/{slot}.png";
}
