using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LifeManager.Data;
using LifeManager.Services.Interfaces;

namespace LifeManager.Services;

public class TagSeederService(IDbContextFactory<AppDbContext> factory) : ITagSeederService
{
    public async Task SeedInitialTagsForHomeAsync(int homeId)
    {
        await using var context = await factory.CreateDbContextAsync();
        
        var hasTags = await context.Tags.AnyAsync(t => t.HomeId == homeId);
        if (hasTags) return;
        
        var initialTags = new List<Tag>
        {
            new Tag { Label = "Basse énergie", ColorHex = "#C8C6E5", HomeId = homeId },
            new Tag { Label = "Énergie moyenne", ColorHex = "#7FB3D5", HomeId = homeId },
            new Tag { Label = "Haute énergie", ColorHex = "#F4A261", HomeId = homeId },
            new Tag { Label = "2–5 min", ColorHex = "#A8E6CF", HomeId = homeId },
            new Tag { Label = "10 min", ColorHex = "#56C596", HomeId = homeId },
            new Tag { Label = "20+ min", ColorHex = "#2E8B57", HomeId = homeId },
            new Tag { Label = "Bloquant", ColorHex = "#E9C46A", HomeId = homeId },
            new Tag { Label = "Visible rapide", ColorHex = "#4ECDC4", HomeId = homeId },
            new Tag { Label = "Soulage l’anxiété", ColorHex = "#90CAF9", HomeId = homeId },
        };

        context.Tags.AddRange(initialTags);
        await context.SaveChangesAsync();
    }
}