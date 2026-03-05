using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace LifeManager.Data;

public class TaskCompletion
{
    public int Id { get; set; }
    public int HouseTaskId { get; set; }
    public int CompletedById { get; set; }
    public User CompletedBy { get; set; }
    public HouseTask HouseTask { get; set; }
    public int XpEarned { get; set; } = 0;
    public DateTime CompletedAt { get; set; }

}