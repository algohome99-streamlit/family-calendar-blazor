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
        var payload = new
        {
            action = "addEvent",
            date = evt.Date.ToString("yyyy-MM-dd"),
            member = evt.Member,
            content = evt.Content,
            type = evt.Type,
            recurring = evt.Recurring
        };
        await _httpClient.PostAsJsonAsync(_gasUrl, payload);
    }

    public async Task UpdateEventAsync(Event evt)
    {
        var payload = new
        {
            action = "updateEvent",
            rowIndex = evt.RowIndex,
            date = evt.Date.ToString("yyyy-MM-dd"),
            member = evt.Member,
            content = evt.Content,
            type = evt.Type,
            recurring = evt.Recurring
        };
        await _httpClient.PostAsJsonAsync(_gasUrl, payload);
    }

    public async Task DeleteEventAsync(int rowIndex)
    {
        var payload = new { action = "deleteEvent", rowIndex };
        await _httpClient.PostAsJsonAsync(_gasUrl, payload);
    }

    public async Task CompleteEventAsync(Event evt)
    {
        var payload = new
        {
            action = "completeEvent",
            rowIndex = evt.RowIndex,
            date = evt.Date.ToString("yyyy-MM-dd"),
            member = evt.Member,
            content = evt.Content,
            type = evt.Type,
            recurring = evt.Recurring
        };
        await _httpClient.PostAsJsonAsync(_gasUrl, payload);
    }

    // === Goals ===
    public async Task<List<Goal>> GetGoalsAsync()
    {
        var response = await _httpClient.GetFromJsonAsync<GasResponse<List<Goal>>>($"{_gasUrl}?action=getGoals");
        return response?.Data ?? new List<Goal>();
    }

    public async Task AddGoalAsync(Goal goal)
    {
        var payload = new
        {
            action = "addGoal",
            member = goal.Member,
            goal = goal.GoalName,
            deadline = goal.Deadline.ToString("yyyy-MM-dd"),
            sub1 = goal.Sub1,
            sub1_pct = goal.Sub1Pct,
            sub2 = goal.Sub2,
            sub2_pct = goal.Sub2Pct,
            sub3 = goal.Sub3,
            sub3_pct = goal.Sub3Pct
        };
        await _httpClient.PostAsJsonAsync(_gasUrl, payload);
    }

    public async Task UpdateGoalAsync(Goal goal)
    {
        var payload = new
        {
            action = "updateGoal",
            rowIndex = goal.RowIndex,
            member = goal.Member,
            goal = goal.GoalName,
            deadline = goal.Deadline.ToString("yyyy-MM-dd"),
            sub1 = goal.Sub1,
            sub1_pct = goal.Sub1Pct,
            sub2 = goal.Sub2,
            sub2_pct = goal.Sub2Pct,
            sub3 = goal.Sub3,
            sub3_pct = goal.Sub3Pct
        };
        await _httpClient.PostAsJsonAsync(_gasUrl, payload);
    }

    public async Task DeleteGoalAsync(int rowIndex)
    {
        var payload = new { action = "deleteGoal", rowIndex };
        await _httpClient.PostAsJsonAsync(_gasUrl, payload);
    }

    public async Task CompleteGoalAsync(Goal goal)
    {
        var payload = new
        {
            action = "completeGoal",
            rowIndex = goal.RowIndex,
            member = goal.Member,
            goal = goal.GoalName
        };
        await _httpClient.PostAsJsonAsync(_gasUrl, payload);
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
