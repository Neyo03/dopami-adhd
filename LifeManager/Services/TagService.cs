using Microsoft.EntityFrameworkCore;
using LifeManager.Data;
using LifeManager.Model;

namespace LifeManager.Services;

public class TagService(IDbContextFactory<AppDbContext> factory)
{
    public async Task<List<TagDto>?> GetTagsAsync(int homeId)
    {
        await using var context = await factory.CreateDbContextAsync();
        var home = await context.Homes.Include(home => home.Tags).FirstOrDefaultAsync( home => home.Id == homeId);
        return home?.Tags.Select(tag => new TagDto()
        {
            HomeId =  tag.HomeId,
            ColorHex = tag.ColorHex,
            Label =  tag.Label,
            TagId = tag.Id
        } ).ToList(); 
    }

    public async Task AddTagAsync(Tag tag)
    {
        await using var context = await factory.CreateDbContextAsync();
        context.Tags.Add(tag);
        await context.SaveChangesAsync();
    }
}