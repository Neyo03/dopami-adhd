using Microsoft.EntityFrameworkCore;

namespace LifeManager.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    
    public DbSet<Home> Homes { get; set; }
    public DbSet<Room> Rooms { get; set; }
    public DbSet<HouseTask> HouseTasks { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<TaskCompletion> TaskCompletions { get; set; }
}