using LifeManager.Data;

namespace LifeManager.Model;

public class TaskDetailsDto
{
    public int TaskId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; } = new DateTime();
    public bool IsDone { get; set; } = false;
    public string? AssignedUsername { get; set; }
    public int RoomId { get; set; }
    public string RoomName { get; set; } = string.Empty;
    public List<Tag> Tags { get; set; } = new();
}