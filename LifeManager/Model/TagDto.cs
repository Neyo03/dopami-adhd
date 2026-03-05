namespace LifeManager.Model;

public class TagDto
{
    public int TagId { get; set; }
    public string Label { get; set; } = "";
    public string ColorHex { get; set; } = "#3b82f6";
    public int HomeId { get; set; }
    public List<TaskDetailsDto> Tasks { get; set; } = new();
}