using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace LifeManager.Data;

public class TaskCompletion
{
    public int Id { get; set; }
    
    public User CompletedBy { get; set; } = new();
    
    public HouseTask HouseTask { get; set; } = new();

    public int XpEarned { get; set; } = 0;
    
    public DateTime? CompletedAt { get; set; }

}