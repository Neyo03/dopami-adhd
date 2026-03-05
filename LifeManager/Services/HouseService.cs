using LifeManager.Extensions;
using LifeManager.Model;
using Microsoft.VisualBasic;

namespace LifeManager.Services;

using Microsoft.EntityFrameworkCore;
using Data;

public class HouseService(IDbContextFactory<AppDbContext> factory)
{
    
    // Récupérer les pièces et leurs tâches
    public async Task<List<Room>> GetRoomsAsync(int homeId)
    {
        await using var context = await factory.CreateDbContextAsync();
        return await context.Rooms.GetRoomsByHome(homeId).ToListAsync();
    }
    
    public async Task<List<RoomDashboardDto>> GetRoomsInprogressTasksOptimizedAsync(int homeId)
    {
        await using var context = await factory.CreateDbContextAsync();
        
        return await context.Rooms
            .GetRoomsByHome(homeId)
            .AsNoTracking()
            .Where(room => room.Tasks.Any(task => !task.IsDone))
            .Select(room => new RoomDashboardDto
            {
                RoomId = room.Id,
                RoomName = room.Name,
                HomeUsers = room.Home!.Users.Select(u => new UserDto 
                { 
                    UserId = u.Id, 
                    Username = u.Username,
                    HomeId =  u.Home.Id,
                    TotalXp = u.TotalXp
                }).ToList(),
                
                ActiveTasks = room.Tasks
                    .Where(task => !task.IsDone)
                    .Select(task => new TaskDetailsDto
                    {
                        TaskId = task.Id,
                        Title = task.Title,
                        AssignedUsername = task.UserAssigned != null ? task.UserAssigned.Username : null,
                        RoomId = task.RoomId,
                        RoomName = task.Room.Name,
                        Tags = task.Tags.ToList(),
                        DueDate = task.DueDate,
                        Description =  task.Description,
                        Energy = task.Energy,
                        Duration =  task.Duration,
                        Impact =  task.Impact,
                        IsDone = task.IsDone
                    }).ToList()
            })
            .ToListAsync();
    }
    
    public async Task<List<TaskDetailsDto>> GetAssignedTasksAsync(int userId)
    {
        await using var context = await factory.CreateDbContextAsync();
        
        return await context.HouseTasks
            .AsNoTracking()
            .Where(task => task.UserAssignedId == userId && !task.IsDone)
            .Select(task => new TaskDetailsDto
            {
                TaskId = task.Id,
                Title = task.Title,
                AssignedUsername = task.UserAssigned != null ? task.UserAssigned.Username : null,
                RoomId = task.RoomId,
                RoomName = task.Room.Name,
                Tags = task.Tags.ToList(),
                DueDate = task.DueDate,
                Description =  task.Description,
                IsDone = task.IsDone
            })
            .ToListAsync();
    }
    
    public async Task<int> CountAssignedTasksAsync(int userId)
    {
        await using var context = await factory.CreateDbContextAsync();
        
        return await context.HouseTasks
            .AsNoTracking()
            .Where(task => task.UserAssignedId == userId && !task.IsDone)
            .CountAsync();
    }
    
    // Récupérer les pièces et leurs tâches terminées de la semaine 
    public async Task<List<DailyUserTasksDto>> GetRoomsDoneTasksWeekAsync(int homeId)
    {
        await using var context = await factory.CreateDbContextAsync();
        
        var today = DateTime.UtcNow.Date;
        int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
        var startOfWeek = today.AddDays(-1 * diff).ToUniversalTime();
        
        var rawCompletions = await context.TaskCompletions
            .AsNoTracking()
            .Where(c => c.HouseTask.Room.HomeId == homeId && c.CompletedAt >= startOfWeek)
            .Select(c => new TaskCompletionDto
            {
                TaskCompletionId = c.Id,
                HouseTaskId = c.HouseTaskId,
                TaskTitle = c.HouseTask.Title, 
                CompletedAt = c.CompletedAt,
                CompletedById = c.CompletedById,
                CompletedByName = c.CompletedBy.Username,
                XpEarned = c.XpEarned,
            })
            .ToListAsync();
        
        var groupedResult = rawCompletions
            .GroupBy(c => new { Date = c.CompletedAt.Date, Username = c.CompletedByName })
            .OrderByDescending(g => g.Key.Date)
            .ThenBy(g => g.Key.Username)
            .Select(g => new DailyUserTasksDto
            {
                Date = g.Key.Date,
                DateFormat = g.Key.Date.ToString("d/mM"),
                Username = g.Key.Username,
                Tasks = g.ToList()
            })
            .ToList();

        return groupedResult;
    }
    
    // Récupérer le nombre de tasks
    public async Task<int> GetTotalTasksAsync(int homeId)
    {
        await using var context = await factory.CreateDbContextAsync();
        return await context.HouseTasks
            .GetTasksByHome(homeId)!
            .AsNoTracking() 
            .CountAsync();
    }
    
    // Récupérer le nombre de tasks terminées
    public async Task<int> GetTotalDoneTasksAsync(int homeId)
    {
        await using var context = await factory.CreateDbContextAsync();
        return await context.HouseTasks
            .GetTasksByHome(homeId)!
            .Where(task => task.IsDone)
            .AsNoTracking() 
            .CountAsync();
    }

    // Ajouter une tâche
    public async Task AddTaskAsync(TaskFormModel formModel)
    {
        await using var context = await factory.CreateDbContextAsync();
        
        var newTask = new HouseTask
        {
            Title = formModel.Title,
            Description = formModel.Description,
            // DueDate = formModel.DueDate,
            RoomId = formModel.RoomId,
            IsDone = formModel.IsDone,
            UserAssignedId = null,
            // Tags = formModel.Tags.ToList(),
            UserAssigned = null,
            Impact = formModel.Impact,
            Energy =  formModel.Energy,
            Duration =  formModel.Duration,
        };
        
        if (formModel.Tags.Any())
        {
            var tagIds = formModel.Tags.Select(t => t.Id).ToList();
            var trackedTags = await context.Tags.Where(t => tagIds.Contains(t.Id)).ToListAsync();
            newTask.Tags.AddRange(trackedTags);
        }

        context.HouseTasks.Add(newTask);
        await context.SaveChangesAsync();
    }
    
    public async Task RemoveTaskAsync(int taskId)
    {
        await using var context = await factory.CreateDbContextAsync();
        
        // Ultra-fast SQL Delete without loading the entity into RAM
        await context.HouseTasks
            .Where(t => t.Id == taskId)
            .ExecuteDeleteAsync();
    }
    
    public async Task UpdateTaskAsync(TaskFormModel formModel)
    {
        if (formModel.Id == null) return;

        await using var context = await factory.CreateDbContextAsync();
        
        var existingTask = await context.HouseTasks
            .Include(task => task.Tags)
            .FirstOrDefaultAsync(task => task.Id == formModel.Id);

        if (existingTask != null)
        {
            existingTask.Title = formModel.Title;
            existingTask.Description = formModel.Description;
            // existingTask.DueDate = formModel.DueDate;
            existingTask.RoomId = formModel.RoomId;
            existingTask.IsDone = formModel.IsDone;
            existingTask.Duration = formModel.Duration;
            existingTask.Impact = formModel.Impact;
            existingTask.Energy = formModel.Energy;
            
            existingTask.Tags.Clear();
            var tagIds = formModel.Tags.Select(t => t.Id).ToList();
            var trackedTags = await context.Tags.Where(t => tagIds.Contains(t.Id)).ToListAsync();
            existingTask.Tags.AddRange(trackedTags);
            
            await context.SaveChangesAsync();
        }
    }
    
    // Créér la maison
    public async Task CreateHomeAsync(Home home)
    {
        //todo vérifier si le nom de la maison existe déjà 
        await using var context = await factory.CreateDbContextAsync();
        
        context.Homes.Add(home);
        await context.SaveChangesAsync();
    }
    
    // Fonction de contournement pour éviter de refacto le service de registration 
    public async Task DeleteHomeAsync(Home home)
    {
        await using var context = await factory.CreateDbContextAsync();
        
        context.Homes.Remove(home);
        await context.SaveChangesAsync();
    }
    
    public async Task ToggleTaskAsync(int taskId, bool toggleTask)
    {
        await using var context = await factory.CreateDbContextAsync();
        
        await context.HouseTasks
            .Where(task => task.Id == taskId)
            .ExecuteUpdateAsync(
                setters => 
                    setters
                        .SetProperty(t => t.IsDone, !toggleTask)
                        .SetProperty(t => t.UserAssignedId, (int?)null)
                    );
    }
    
    public async Task AssignUserTaskAsync(int taskId, int? userId)
    {
        await using var context = await factory.CreateDbContextAsync();
        
        await context.HouseTasks
            .Where(task => task.Id == taskId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(t => t.UserAssignedId, userId));
    }
    
    public async Task CreateTaskCompletionAsync(TaskCompletionModel taskCompletion)
    {
        await using var context = await factory.CreateDbContextAsync();
        
        var newTaskCompletion = new TaskCompletion
        {
           HouseTaskId = taskCompletion.HouseTaskId,
           CompletedById = taskCompletion.CompletedById,
           CompletedAt = taskCompletion.CompletedAt,
           XpEarned = taskCompletion.XpEarned,
        };
        
        context.TaskCompletions.Add(newTaskCompletion);
        await context.SaveChangesAsync();
    }
}