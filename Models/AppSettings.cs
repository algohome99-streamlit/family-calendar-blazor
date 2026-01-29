namespace FamilyCalendar.Blazor.Models;

public class AppSettings
{
    public Dictionary<string, int> PointsMap { get; set; } = new()
    {
        { "日常行程", 1 },
        { "重要提醒", 2 },
        { "功課", 2 },
        { "考試", 3 },
        { "社團", 2 },
        { "才藝", 2 },
        { "家庭活動", 3 },
        { "重大事件(期末考/校外教學)", 5 },
        { "長期目標", 10 }
    };

    public int PointsGoal { get; set; } = 50;
    public string Password { get; set; } = "1234";

    public int GetPoints(string type)
    {
        return PointsMap.TryGetValue(type, out var points) ? points : 1;
    }
}
