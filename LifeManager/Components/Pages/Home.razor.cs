using LifeManager.Data;
using LifeManager.Model;
using Microsoft.AspNetCore.Components;

namespace LifeManager.Components.Pages;

public partial class Home : ComponentBase
{
    private List<Room>? _rooms;
    private List<Room>? _roomsWithTasks;
    private List<Room>? _roomsWithDoneTasks;

    private User? _connectedUser;
    private UserLevelModel? _levelingUser;

    
    private HouseTask _currentTask = new();
    
    private int _countTasks;
    private int _countDoneTasks;
    
    private bool _isDrawerOpen;
    private bool _isEditMode;
    private bool _isOpenDoneTask;


    protected override async Task OnInitializedAsync()
    {
        _connectedUser = await UserService.GetAuthenticatedUserAsync();
        _levelingUser = await LevelingService.CalculateLevelAsync(_connectedUser!.TotalXp);
        TagState.OnChange += StateHasChanged;
        await TagState.InitializeAsync(_connectedUser);
        await LoadDataAsync();
    }
    
    public void Dispose()
    {
        TagState.OnChange -= StateHasChanged;
    }

    private async Task LoadDataAsync()
    {
        
        _rooms = await HouseService.GetRoomsAsync(_connectedUser);
        _roomsWithTasks = await HouseService.GetRoomsInprogressTasksAsync(_connectedUser);
        _countTasks = await HouseService.GetTotalTasksAsync(_connectedUser);
        _countDoneTasks = await HouseService.GetTotalDoneTasksAsync(_connectedUser);
        _roomsWithDoneTasks = await HouseService.GetRoomsDoneTasksAsync(_connectedUser);
        _levelingUser = await LevelingService.CalculateLevelAsync(_connectedUser!.TotalXp);
    }

    private void OpenCreateDrawer()
    {
        _currentTask = new HouseTask();
        _isEditMode = false;
        _isDrawerOpen = true;
    }

    private void ToggleDoneTasksSection()
    {
        _isOpenDoneTask = !_isOpenDoneTask;
    }

    private void OpenEditDrawer(HouseTask task)
    {
        _currentTask = new HouseTask 
        { 
            Id = task.Id,
            Title = task.Title, 
            Description = task.Description,
            DueDate = task.DueDate,
            Room = task.Room,
            IsDone = task.IsDone,
            Tags = task.Tags.ToList()
        };
        _isEditMode = true;
        _isDrawerOpen = true;
    }

    private void CloseDrawer()
    {
        _isDrawerOpen = false;
        _currentTask = new HouseTask();
    }

    private async Task SaveTask()
    {
        if (_isEditMode)
        {
            await HouseService.UpdateTaskAsync(_currentTask);
        }
        else
        {
            await HouseService.AddTaskAsync(_currentTask);
        }
        
        await LoadDataAsync();
        CloseDrawer();
    }
    
    private async Task RemoveTask()
    {
        await HouseService.RemoveTaskAsync(_currentTask);
        await LoadDataAsync();
        CloseDrawer();
    }

    private async Task ToggleTask(HouseTask task)
    {
        await HouseService.ToggleTaskAsync(task);
        await TaskCompleted(task);
        await UserService.UpdateTotalXpUser(_connectedUser);
        _connectedUser = await UserService.GetAuthenticatedUserAsync();
        _levelingUser = await LevelingService.CalculateLevelAsync(_connectedUser.TotalXp);
        _roomsWithTasks = await HouseService.GetRoomsInprogressTasksAsync(_connectedUser);
        _countDoneTasks = await HouseService.GetTotalDoneTasksAsync(_connectedUser);
        _roomsWithDoneTasks = await HouseService.GetRoomsDoneTasksAsync(_connectedUser);
    }
    
    private async Task TaskCompleted(HouseTask task)
    {
        var taskCompletion = new TaskCompletion()
        {
            CompletedAt = new DateTime(),
            HouseTask = task,
            CompletedBy = _connectedUser!,
            XpEarned = 10,
        };

        await HouseService.CreateTaskCompletionAsync(taskCompletion);
    }
}