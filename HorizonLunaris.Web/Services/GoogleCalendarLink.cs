using System.Globalization;

namespace HorizonLunaris.Web.Services;

public static class GoogleCalendarLink
{
    public static string Create(string title, DateTime dueDateUtc)
    {
        var startDate = dueDateUtc.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
        var endDate = dueDateUtc.Date.AddDays(1).ToString("yyyyMMdd", CultureInfo.InvariantCulture);
        var eventTitle = Uri.EscapeDataString(title);
        var eventDates = Uri.EscapeDataString($"{startDate}/{endDate}");

        return $"https://calendar.google.com/calendar/render?action=TEMPLATE&text={eventTitle}&dates={eventDates}";
    }
}
