using System.Net.Http.Json;
using FamilyCalendar.Blazor.Models;
using Microsoft.Extensions.Configuration;

namespace FamilyCalendar.Blazor.Services;

public class DataService : IDataService
{
    private readonly HttpClient _httpClient;
    private readonly string _gasUrl;

    public DataService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _gasUrl = configuration["GasWebAppUrl"] ?? throw new InvalidOperationException("GasWebAppUrl not configured");
    }

    // === Events ===
    public async Task<List<Event>> GetEventsAsync()
    {
        var response = await _httpClient.GetFromJsonAsync<GasResponse<List<Event>>>($"{_gasUrl}?action=getEvents");
        return response?.Data ?? new List<Event>();
    }

    public async Task AddEventAsync(Event evt)
    {
        var url = $"{_gasUrl}?action=addEvent" +
            $"&date={Uri.EscapeDataString(evt.Date.ToString("yyyy-MM-dd"))}" +
            $"&member={Uri.EscapeDataString(evt.Member)}" +
            $"&content={Uri.EscapeDataString(evt.Content)}" +
            $"&type={Uri.EscapeDataString(evt.Type)}" +
            $"&recurring={Uri.EscapeDataString(evt.Recurring)}";
        await _httpClient.GetAsync(url);
    }

    public async Task UpdateEventAsync(Event evt)
    {
        var url = $"{_gasUrl}?action=updateEvent" +
            $"&rowIndex={evt.RowIndex}" +
            $"&date={Uri.EscapeDataString(evt.Date.ToString("yyyy-MM-dd"))}" +
            $"&member={Uri.EscapeDataString(evt.Member)}" +
            $"&content={Uri.EscapeDataString(evt.Content)}" +
            $"&type={Uri.EscapeDataString(evt.Type)}" +
            $"&recurring={Uri.EscapeDataString(evt.Recurring)}";
        await _httpClient.GetAsync(url);
    }

    public async Task DeleteEventAsync(int rowIndex)
    {
        var url = $"{_gasUrl}?action=deleteEvent&rowIndex={rowIndex}";
        await _httpClient.GetAsync(url);
    }

    public async Task CompleteEventAsync(Event evt)
    {
        var url = $"{_gasUrl}?action=completeEvent" +
            $"&rowIndex={evt.RowIndex}" +
            $"&date={Uri.EscapeDataString(evt.Date.ToString("yyyy-MM-dd"))}" +
            $"&member={Uri.EscapeDataString(evt.Member)}" +
            $"&content={Uri.EscapeDataString(evt.Content)}" +
            $"&type={Uri.EscapeDataString(evt.Type)}" +
            $"&recurring={Uri.EscapeDataString(evt.Recurring)}";
        await _httpClient.GetAsync(url);
    }

    // === Goals ===
    public async Task<List<Goal>> GetGoalsAsync()
    {
        var response = await _httpClient.GetFromJsonAsync<GasResponse<List<Goal>>>($"{_gasUrl}?action=getGoals");
        return response?.Data ?? new List<Goal>();
    }

    public async Task AddGoalAsync(Goal goal)
    {
        var url = $"{_gasUrl}?action=addGoal" +
            $"&member={Uri.EscapeDataString(goal.Member)}" +
            $"&goal={Uri.EscapeDataString(goal.GoalName)}" +
            $"&deadline={Uri.EscapeDataString(goal.Deadline.ToString("yyyy-MM-dd"))}" +
            $"&sub1={Uri.EscapeDataString(goal.Sub1)}" +
            $"&sub1_pct={goal.Sub1Pct}" +
            $"&sub2={Uri.EscapeDataString(goal.Sub2)}" +
            $"&sub2_pct={goal.Sub2Pct}" +
            $"&sub3={Uri.EscapeDataString(goal.Sub3)}" +
            $"&sub3_pct={goal.Sub3Pct}";
        await _httpClient.GetAsync(url);
    }

    public async Task UpdateGoalAsync(Goal goal)
    {
        var url = $"{_gasUrl}?action=updateGoal" +
            $"&rowIndex={goal.RowIndex}" +
            $"&member={Uri.EscapeDataString(goal.Member)}" +
            $"&goal={Uri.EscapeDataString(goal.GoalName)}" +
            $"&deadline={Uri.EscapeDataString(goal.Deadline.ToString("yyyy-MM-dd"))}" +
            $"&sub1={Uri.EscapeDataString(goal.Sub1)}" +
            $"&sub1_pct={goal.Sub1Pct}" +
            $"&sub2={Uri.EscapeDataString(goal.Sub2)}" +
            $"&sub2_pct={goal.Sub2Pct}" +
            $"&sub3={Uri.EscapeDataString(goal.Sub3)}" +
            $"&sub3_pct={goal.Sub3Pct}";
        await _httpClient.GetAsync(url);
    }

    public async Task DeleteGoalAsync(int rowIndex)
    {
        var url = $"{_gasUrl}?action=deleteGoal&rowIndex={rowIndex}";
        await _httpClient.GetAsync(url);
    }

    public async Task CompleteGoalAsync(Goal goal)
    {
        var url = $"{_gasUrl}?action=completeGoal" +
            $"&rowIndex={goal.RowIndex}" +
            $"&member={Uri.EscapeDataString(goal.Member)}" +
            $"&goal={Uri.EscapeDataString(goal.GoalName)}";
        await _httpClient.GetAsync(url);
    }

    // === History ===
    public async Task<List<HistoryRecord>> GetHistoryAsync()
    {
        var response = await _httpClient.GetFromJsonAsync<GasResponse<List<HistoryRecord>>>($"{_gasUrl}?action=getHistory");
        return response?.Data ?? new List<HistoryRecord>();
    }

    // === Settings ===
    public async Task<AppSettings> GetSettingsAsync()
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<GasResponse<Dictionary<string, string>>>($"{_gasUrl}?action=getSettings");
            var settings = new AppSettings();

            if (response?.Data != null)
            {
                foreach (var kvp in response.Data)
                {
                    if (kvp.Key.StartsWith("points_") && kvp.Key != "points_goal")
                    {
                        var type = kvp.Key.Replace("points_", "");
                        if (int.TryParse(kvp.Value, out var points))
                        {
                            settings.PointsMap[type] = points;
                        }
                    }
                    else if (kvp.Key == "points_goal" && int.TryParse(kvp.Value, out var goal))
                    {
                        settings.PointsGoal = goal;
                    }
                    else if (kvp.Key == "password")
                    {
                        settings.Password = kvp.Value;
                    }
                }
            }

            return settings;
        }
        catch
        {
            return new AppSettings();
        }
    }
}

public class GasResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Error { get; set; }
}
