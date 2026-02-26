using System.Security.Claims;
using LifeManager.Data;
using LifeManager.Extensions;
using Microsoft.EntityFrameworkCore;

namespace LifeManager.Services;

public class UserService(IDbContextFactory<AppDbContext> factory, IHttpContextAccessor httpContext)
{
    public async Task<User?> AuthenticateAsync(string email, string password)
    {
        await using var context = await factory.CreateDbContextAsync();
        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == email);
        
        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.Password))
            return null;

        return user;
    }

    public async Task<bool> RegisterAsync(User user)
    {
        await using var context = await factory.CreateDbContextAsync();
        
        if (await context.Users.AnyAsync(u => u.Email == user.Email))
            return false;

        user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return true;
    }
    
    public async Task<User?> GetAuthenticatedUserAsync()
    {  
        await using var context = await factory.CreateDbContextAsync();

        int userId =  Convert.ToInt32(httpContext.HttpContext?.User.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)
            ?.Value);
        
        return await context.Users.Include(user => user.Home).FirstOrDefaultAsync(user => user.Id == userId);
    }
    
    public async Task UpdateTotalXpUser(User user)
    {
        await using var context = await factory.CreateDbContextAsync();
        
        var totalXpTasks = await context.TaskCompletions
            .GetCompletedTaskByUser(user)!
            .SumAsync(completion => completion.XpEarned);
   

        await context.Users
            .Where(userToUpdate => userToUpdate.Id == user.Id)
            .ExecuteUpdateAsync(setters => setters.SetProperty(u => u.TotalXp, totalXpTasks));
        
    }
}