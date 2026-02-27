using System.ComponentModel.DataAnnotations;
using LifeManager.Data;

namespace LifeManager.Model;

public class TaskCompletionModel
{
    public int TaskCompletionId { get; set; }
    public int HouseTaskId { get; set; }
    public int CompletedById { get; set; }
    public int XpEarned { get; set; } = 0;
    public DateTime? CompletedAt { get; set; }
}