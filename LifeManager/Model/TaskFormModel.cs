using System.ComponentModel.DataAnnotations;
using LifeManager.Data;

namespace LifeManager.Model;

public class TaskFormModel
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Title is required")]
    public string Title { get; set; } = string.Empty;
    
    public string? Description { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a room")]
    public int RoomId { get; set; }
    
    public bool IsDone { get; set; }
    public List<Tag> Tags { get; set; } = new();
}