using System.ComponentModel.DataAnnotations;

namespace LifeManager.Data;

public class Room
{
    // Todo: make dto for form and dto for read data
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public List<HouseTask> Tasks { get; set; } = new();

    [Required(ErrorMessage = "Une maison est obligatoire.")]
    public Home Home { get; set; }
    
    public int HomeId { get; set; }
}