namespace FamilyCalendar.Blazor.Models;

public class Event
{
    public int RowIndex { get; set; }
    public DateTime Date { get; set; }
    public string Member { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool IsDone { get; set; }
    public string Recurring { get; set; } = string.Empty;

    public bool IsRecurring => Recurring == "是";

    public string GetIcon() => Type switch
    {
        "日常行程" => "📅",
        "重要提醒" => "⚠️",
        "考試" => "📖",
        "社團" => "🏃",
        "家庭活動" => "👨‍👩‍👧‍👦",
        _ => "📅"
    };

    public string GetCardClass() => Member switch
    {
        "哥哥" => "card-bro",
        "妹妹" => "card-sis",
        "全家" => "card-family",
        _ => ""
    };

    public string GetMemberEmoji() => Member switch
    {
        "哥哥" => "👦",
        "妹妹" => "👧",
        "全家" => "👨‍👩‍👧‍👦",
        _ => "👤"
    };

    public static readonly string[] EventTypes =
    {
        "日常行程", "重要提醒",  "考試",
        "社團",  "家庭活動"
    };

    public static readonly string[] Members = { "哥哥", "妹妹", "全家" };
}
