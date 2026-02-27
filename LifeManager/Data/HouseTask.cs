using System.ComponentModel.DataAnnotations;

namespace LifeManager.Data;

public class HouseTask
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Un titre est obligatoire.")]
    public string Title { get; set; } = "";
    
    public string? Description { get; set; } = "";
    
    public DateTime? DueDate { get; set; }
    
    public bool IsDone { get; set; }
    
    public int RoomId { get; set; }
    
    public Room Room { get; set; }
    
    public List<Tag> Tags { get; set; } = new();
    
    public User? UserAssigned { get; set; }
    
    public int? UserAssignedId { get; set; }

    public int XpToEarn { get; set; } = 0;
}