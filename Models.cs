using System;

namespace ScheduleTasksApp;

public sealed class DateItem
{
    public int Id { get; set; }
    public DateOnly Date { get; set; }
    public string DisplayText => Date.ToString("yyyy-MM-dd (ddd)");
}

public sealed class TaskItem
{
    public int Id { get; set; }
    public int DateId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Assignee { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
    public bool IsDone { get; set; }
}
