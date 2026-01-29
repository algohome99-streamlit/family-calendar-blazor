namespace FamilyCalendar.Blazor.Models;

public class Goal
{
    public int RowIndex { get; set; }
    public string Member { get; set; } = string.Empty;
    public string GoalName { get; set; } = string.Empty;
    public DateTime Deadline { get; set; }
    public string Sub1 { get; set; } = string.Empty;
    public int Sub1Pct { get; set; }
    public string Sub2 { get; set; } = string.Empty;
    public int Sub2Pct { get; set; }
    public string Sub3 { get; set; } = string.Empty;
    public int Sub3Pct { get; set; }

    public string GetMemberEmoji() => Member switch
    {
        "哥哥" => "👦",
        "妹妹" => "👧",
        "全家" => "👨‍👩‍👧‍👦",
        _ => "👤"
    };

    public string GetCardClass() => Member switch
    {
        "哥哥" => "card-bro",
        "妹妹" => "card-sis",
        "全家" => "card-family",
        _ => ""
    };

    public List<(string Name, int Percent)> GetSubItems()
    {
        var items = new List<(string, int)>();
        if (!string.IsNullOrWhiteSpace(Sub1)) items.Add((Sub1, Sub1Pct));
        if (!string.IsNullOrWhiteSpace(Sub2)) items.Add((Sub2, Sub2Pct));
        if (!string.IsNullOrWhiteSpace(Sub3)) items.Add((Sub3, Sub3Pct));
        return items;
    }
}
