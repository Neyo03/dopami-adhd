using LifeManager.Extensions;
using LifeManager.Model;

namespace LifeManager.Services;

using Microsoft.EntityFrameworkCore;
using Data;

public class HouseService(IDbContextFactory<AppDbContext> factory)
{
    
    // Récupérer les pièces et leurs tâches
    public async Task<List<RoomDashboardDto>> GetRoomsAsync(int homeId)
    {
        await using var context = await factory.CreateDbContextAsync();
        return await context.Rooms
            .GetRoomsByHome(homeId)
            .AsNoTracking()
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
            })
            .ToListAsync();
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
                        Tags = task.Tags.Select(tag => new TagDto { TagId = tag.Id, Label = tag.Label, ColorHex = tag.ColorHex, HomeId = tag.HomeId }).ToList(),
                        DueDate = task.DueDate,
                        Description =  task.Description,
                        Energy = task.Energy,
                        Duration =  task.Duration,
                        Impact =  task.Impact,
                        IsDone = task.IsDone,
                        XpToEarn = task.XpToEarn,
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
                Tags = task.Tags.Select(tag => new TagDto { TagId = tag.Id, Label = tag.Label, ColorHex = tag.ColorHex, HomeId = tag.HomeId }).ToList(),
                DueDate = task.DueDate,
                Description =  task.Description,
                Energy = task.Energy,
                Duration =  task.Duration,
                Impact =  task.Impact,
                IsDone = task.IsDone,
                XpToEarn = task.XpToEarn,
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
    public async Task AddTaskAsync(TaskFormModel formModel, int calculateXp)
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
            XpToEarn = calculateXp,
        };
        
        if (formModel.Tags.Any())
        {
            var tagIds = formModel.Tags.Select(t => t.TagId).ToList();
            var trackedTags = await context.Tags.Where(t => tagIds.Contains(t.Id)).ToListAsync();
            newTask.Tags.AddRange(trackedTags);
        }

        context.HouseTasks.Add(newTask);
        await context.SaveChangesAsync();
    }
    
    public async Task RemoveTaskAsync(int taskId, int userHomeId)
    {
        await using var context = await factory.CreateDbContextAsync();
        
        await context.HouseTasks
            .Where(t => t.Id == taskId && t.Room.HomeId == userHomeId)
            .ExecuteDeleteAsync();
    }
    
    // Note : contrairement aux autres mutations qui utilisent ExecuteUpdateAsync,
    // cette méthode charge l'entité en mémoire. C'est intentionnel : EF Core ne permet
    // pas de gérer la relation many-to-many (Tags) via ExecuteUpdateAsync, car cela
    // nécessite de manipuler la table de jointure implicite (HouseTaskTag).
    public async Task UpdateTaskAsync(TaskFormModel formModel, int userHomeId, int calculateXp)
    {
        await using var context = await factory.CreateDbContextAsync();

        var existingTask = await context.HouseTasks
            .Include(task => task.Tags)
            .FirstOrDefaultAsync(task => task.Id == formModel.Id && task.Room.HomeId == userHomeId);

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
            existingTask.XpToEarn = calculateXp;
            
            existingTask.Tags.Clear();
            var tagIds = formModel.Tags.Select(t => t.TagId).ToList();
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
    
    public async Task ToggleTaskAsync(int taskId, bool toggleTask, int userHomeId)
    {
        await using var context = await factory.CreateDbContextAsync();
        
        await context.HouseTasks
            .Where(task => task.Id == taskId && task.Room.HomeId == userHomeId)
            .ExecuteUpdateAsync(
                setters => 
                    setters
                        .SetProperty(t => t.IsDone, !toggleTask)
                        .SetProperty(t => t.UserAssignedId, (int?)null)
                    );
    }
    
    public async Task AssignUserTaskAsync(int taskId, int userHomeId, int? userId)
    {
        await using var context = await factory.CreateDbContextAsync();
        
        await context.HouseTasks
            .Where(task => task.Id == taskId && task.Room.HomeId == userHomeId)
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
    
    public async Task DeleteRoomAsync(int roomId, int userHomeId)
    {
        await using var context = await factory.CreateDbContextAsync();
        
        await context.Rooms
            .Where(r => r.Id == roomId && r.HomeId == userHomeId)
            .ExecuteDeleteAsync();
    }

}