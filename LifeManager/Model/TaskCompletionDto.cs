using System.ComponentModel.DataAnnotations;
using LifeManager.Data;

namespace LifeManager.Model;

public class TaskCompletionDto
{
    public int TaskCompletionId { get; set; }
    public int HouseTaskId { get; set; }
    public string TaskTitle { get; set; } = string.Empty;
    public int CompletedById { get; set; }
    public string CompletedByName { get; set; } = string.Empty;
    public int XpEarned { get; set; } = 0;
    public DateTime CompletedAt { get; set; }
}