using LifeManager.Extensions;
using LifeManager.Model;

namespace LifeManager.Services;

using Microsoft.EntityFrameworkCore;
using Data;

public class HouseService(IDbContextFactory<AppDbContext> factory)
{
    
    // Récupérer les pièces et leurs tâches
    public async Task<List<Room>> GetRoomsAsync(User user)
    {
        await using var context = await factory.CreateDbContextAsync();
        return await context.Rooms.GetRoomsByHome(user).ToListAsync();
    }
    
    // Récupérer les pièces et leurs tâches non terminées
    public async Task<List<Room>> GetRoomsInprogressTasksAsync(User user)
    {
        await using var context = await factory.CreateDbContextAsync();
        return await context.Rooms
            .GetRoomsByHome(user)
            .AsNoTracking()
            .Include(room => room.Tasks.Where(task => !task.IsDone))
            .ThenInclude(task => task.Tags)
            .Where(room => room.Tasks.Any(task => !task.IsDone))
            .ToListAsync();
    }
    
    // Récupérer les pièces et leurs tâches terminées
    public async Task<List<Room>> GetRoomsDoneTasksAsync(User user)
    {
        await using var context = await factory.CreateDbContextAsync();
        return await context.Rooms
            .GetRoomsByHome(user)
            .AsNoTracking()
            .Include(room => room.Tasks.Where(task => task.IsDone))
            .ThenInclude(task => task.Tags)
            .Where(room => room.Tasks.Any(task => task.IsDone))
            .ToListAsync();
    }
    
    // Récupérer le nombre de tasks
    public async Task<int> GetTotalTasksAsync(User user)
    {
        await using var context = await factory.CreateDbContextAsync();
        return await context.HouseTasks
            .GetTasksByHome(user)!
            .AsNoTracking() 
            .CountAsync();
    }
    
    // Récupérer le nombre de tasks terminées
    public async Task<int> GetTotalDoneTasksAsync(User user)
    {
        await using var context = await factory.CreateDbContextAsync();
        return await context.HouseTasks
            .GetTasksByHome(user)!
            .Where(task => task.IsDone)
            .AsNoTracking() 
            .CountAsync();
    }

    // Ajouter une tâche
    public async Task AddTaskAsync(HouseTask task)
    {
        await using var context = await factory.CreateDbContextAsync();
        foreach (var tag in task.Tags)
        {
            context.Entry(tag).State = EntityState.Unchanged;
        }
        context.HouseTasks.Add(task);
        await context.SaveChangesAsync();
    }
    
    // supprimer une tâche
    public async Task RemoveTaskAsync(HouseTask task)
    {
        await using var context = await factory.CreateDbContextAsync();
        context.HouseTasks.Remove(task);
        await context.SaveChangesAsync();
    }
    
    // Mets à jours la tache en la cherchant de le context et en copiant les valeur de l'objet cloné 
    // car EF Core génère une erreur car il track déjà la tache que je modifie 
    public async Task UpdateTaskAsync(HouseTask taskFromForm)
    {
        await using var context = await factory.CreateDbContextAsync();
        var existingTask = await context.HouseTasks
            .Include(task => task.Tags)
            .FirstOrDefaultAsync(task => task.Id == taskFromForm.Id);

        if (existingTask != null)
        {
            context.Entry(existingTask).CurrentValues.SetValues(taskFromForm);
            
            existingTask.Tags.Clear();
        
            foreach (var tag in taskFromForm.Tags)
            {
                var trackedTag = await context.Tags.FindAsync(tag.Id);
                if (trackedTag != null)
                {
                    existingTask.Tags.Add(trackedTag);
                }
            }
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
    
    public async Task ToggleTaskAsync(HouseTask toggleTask)
    {
        await using var context = await factory.CreateDbContextAsync();
        
        await context.HouseTasks
            .Where(task => task.Id == toggleTask.Id)
            .ExecuteUpdateAsync(setters => setters.SetProperty(t => t.IsDone, !toggleTask.IsDone));
    }
    
    public async Task CreateTaskCompletionAsync(TaskCompletion taskCompletion)
    {
        await using var context = await factory.CreateDbContextAsync();
        
        context.Entry(taskCompletion.HouseTask).State = EntityState.Unchanged;
        context.Entry(taskCompletion.CompletedBy).State = EntityState.Unchanged;
        
        context.TaskCompletions.Add(taskCompletion);
        await context.SaveChangesAsync();
    }
}