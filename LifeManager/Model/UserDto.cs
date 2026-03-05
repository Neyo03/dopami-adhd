namespace LifeManager.Model;

public class UserDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public int TotalXp { get; set; }
    public int HomeId { get; set; }
}