namespace HorizonLunaris.Domain.Entities;

public class EmailNotificationSettings
{
    public int Id { get; set; }
    public string SourceEmail { get; set; } = string.Empty;
}
