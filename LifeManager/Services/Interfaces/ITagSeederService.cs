namespace LifeManager.Services.Interfaces;

public interface ITagSeederService
{
    Task SeedInitialTagsForHomeAsync(int homeId);
}