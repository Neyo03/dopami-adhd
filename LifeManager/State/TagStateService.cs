using LifeManager.Data;
using LifeManager.Model;
using LifeManager.Services;

namespace LifeManager.State;

public class TagStateService(TagService tagService)
{
    public List<TagDto> Tags { get; private set; } = new();
    
    public event Action? OnChange;
    
    public async Task InitializeAsync(UserDto user)
    {
        if (Tags.Count == 0)
        {
            await RefreshAsync(user);
        }
    }
    
    public async Task AddTagAsync(Tag tag)
    {
        await tagService.AddTagAsync(tag);
    }

    public async Task RefreshAsync(UserDto user)
    {
        var fetchedTags = await tagService.GetTagsAsync(user.HomeId);
        Tags = fetchedTags ?? new();
        NotifyStateChanged();
    }

    // Deletes a tag, updates memory, and notifies components
    // public async Task DeleteTagAsync(Tag tag)
    // {
    //     await _tagService.DeleteTagAsync(tag);
    //     Tags.Remove(tag);
    //     NotifyStateChanged();
    // }
    
    private void NotifyStateChanged() => OnChange?.Invoke();
}