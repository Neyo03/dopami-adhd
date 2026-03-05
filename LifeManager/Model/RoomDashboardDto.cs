namespace LifeManager.Model;

public class RoomDashboardDto
{
    public int RoomId { get; set; }
    public string RoomName { get; set; } = string.Empty;
    public List<UserDto> HomeUsers { get; set; } = new();
    public List<TaskDetailsDto> ActiveTasks { get; set; } = new();
}