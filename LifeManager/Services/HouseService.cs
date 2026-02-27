using LifeManager.Extensions;
using LifeManager.Model;

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
    
    // Récupérer les pièces et leurs tâches terminées
    public async Task<List<Room>> GetRoomsDoneTasksAsync(int homeId)
    {
        await using var context = await factory.CreateDbContextAsync();
        return await context.Rooms
            .GetRoomsByHome(homeId)
            .AsNoTracking()
            .Include(room => room.Tasks.Where(task => task.IsDone))
            .ThenInclude(task => task.Tags)
            .Where(room => room.Tasks.Any(task => task.IsDone))
            .ToListAsync();
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
            DueDate = formModel.DueDate,
            RoomId = formModel.RoomId,
            IsDone = formModel.IsDone,
            UserAssignedId = null,
            Tags = formModel.Tags.ToList(),
            UserAssigned = null,
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
            existingTask.DueDate = formModel.DueDate;
            existingTask.RoomId = formModel.RoomId;
            existingTask.IsDone = formModel.IsDone;
            
            existingTask.Tags.Clear();
            var tagIds = formModel.Tags.Select(t => t.Id).ToList();
            var trackedTags = await context.Tags.Where(t => tagIds.Contains(t.Id)).ToListAsync();
            existingTask.Tags.AddRange(trackedTags);
            
            await context.SaveChangesAsync();
        }
    }
    
    // Créér la maison
    public async Task<Home> CreateHomeAsync(Home home)
    {
        await using var context = await factory.CreateDbContextAsync();
        
        context.Homes.Add(home);
        await context.SaveChangesAsync();

        return home;
    }
    
    public async Task ToggleTaskAsync(int taskId, bool toggleTask)
    {
        await using var context = await factory.CreateDbContextAsync();
        
        await context.HouseTasks
            .Where(task => task.Id == taskId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(t => t.IsDone, !toggleTask));
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
        
        var newTaskcompletion = new TaskCompletion
        {
           HouseTaskId = taskCompletion.HouseTaskId,
           CompletedById = taskCompletion.CompletedById,
           CompletedAt = taskCompletion.CompletedAt,
           XpEarned = taskCompletion.XpEarned,
        };
        
        context.TaskCompletions.Add(newTaskcompletion);
        await context.SaveChangesAsync();
    }
}