using FamilyCalendar.Blazor.Models;

namespace FamilyCalendar.Blazor.Services;

public interface IDataService
{
    // Events
    Task<List<Event>> GetEventsAsync();
    Task AddEventAsync(Event evt);
    Task UpdateEventAsync(Event evt);
    Task DeleteEventAsync(int rowIndex);
    Task CompleteEventAsync(Event evt);

    // Goals
    Task<List<Goal>> GetGoalsAsync();
    Task AddGoalAsync(Goal goal);
    Task UpdateGoalAsync(Goal goal);
    Task DeleteGoalAsync(int rowIndex);
    Task CompleteGoalAsync(Goal goal);

    // History
    Task<List<HistoryRecord>> GetHistoryAsync();

    // Settings
    Task<AppSettings> GetSettingsAsync();
}
