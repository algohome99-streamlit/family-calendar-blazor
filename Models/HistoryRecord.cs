namespace FamilyCalendar.Blazor.Models;

public class HistoryRecord
{
    public int RowIndex { get; set; }
    public DateTime Date { get; set; }
    public string Member { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public DateTime CompletedDate { get; set; }
    public int Points { get; set; }

    public string GetIcon() => Type switch
    {
        "日常行程" => "📅",
        "重要提醒" => "⚠️",
        "功課" => "📝",
        "考試" => "📖",
        "社團" => "🏃",
        "才藝" => "🎨",
        "家庭活動" => "👨‍👩‍👧‍👦",
        "重大事件(期末考/校外教學)" => "🚩",
        "長期目標" => "🎯",
        _ => "📅"
    };

    public string GetMemberEmoji() => Member switch
    {
        "哥哥" => "👦",
        "妹妹" => "👧",
        "全家" => "👨‍👩‍👧‍👦",
        _ => "👤"
    };
}
