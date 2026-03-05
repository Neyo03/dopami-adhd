namespace LifeManager.Model;

public class DailyUserTasksDto
{
    public DateTime Date { get; set; }
    
    public string DateFormat { get; set; }= string.Empty;
    public string Username { get; set; } = string.Empty;
    public List<TaskCompletionDto> Tasks { get; set; } = new();
}