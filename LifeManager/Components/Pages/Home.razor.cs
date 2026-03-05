using LifeManager.Data;
using LifeManager.Model;
using Microsoft.AspNetCore.Components;

namespace LifeManager.Components.Pages;

public partial class Home : ComponentBase
{
    private List<Room>? _rooms;
    private List<RoomDashboardDto>? _roomsWithTasks;
    private List<DailyUserTasksDto>? _roomsWithDoneTasks;
    private List<TaskDetailsDto>? _assignedTasks;
    
    private UserDto? _connectedUser;
    private UserLevelModel? _levelingUser;

    
    private TaskFormModel _currentForm = new();
    
    private int _countTasks;
    private int _countDoneTasks;
    private int _countAssignedTasks;
    
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
        _rooms = await HouseService.GetRoomsAsync(_connectedUser.HomeId);
        _roomsWithTasks = await HouseService.GetRoomsInprogressTasksOptimizedAsync(_connectedUser.HomeId);
        _countTasks = await HouseService.GetTotalTasksAsync(_connectedUser.HomeId);
        _countDoneTasks = await HouseService.GetTotalDoneTasksAsync(_connectedUser.HomeId);
        _countAssignedTasks = await HouseService.CountAssignedTasksAsync(_connectedUser.UserId);
        _roomsWithDoneTasks = await HouseService.GetRoomsDoneTasksWeekAsync(_connectedUser.HomeId);
        _levelingUser = await LevelingService.CalculateLevelAsync(_connectedUser.TotalXp);
        _assignedTasks = await HouseService.GetAssignedTasksAsync(_connectedUser.UserId);
    }

    private void OpenCreateDrawer()
    {
        _currentForm = new TaskFormModel();
        _isEditMode = false;
        _isDrawerOpen = true;
    }

    private void ToggleDoneTasksSection()
    {
        _isOpenDoneTask = !_isOpenDoneTask;
    }

    private void OpenEditDrawer(TaskDetailsDto task)
    {
        _currentForm = new TaskFormModel 
        { 
            Id = task.TaskId,
            Title = task.Title, 
            Description = task.Description,
            // DueDate = task.DueDate,
            RoomId = task.RoomId, 
            IsDone = task.IsDone,
            // Tags = task.Tags.ToList(),
            Duration = task.Duration,
            Energy =  task.Energy,
            Impact =  task.Impact,
        };
        _isEditMode = true;
        _isDrawerOpen = true;
    }
    
    private async Task ToggleAssignUserToTask((TaskDetailsDto task, UserDto user)  args)
    {
        bool isSameUser = args.task.AssignedUsername == args.user.Username;
        args.task.AssignedUsername = isSameUser ? null : args.task.AssignedUsername;
        await HouseService.AssignUserTaskAsync(args.task.TaskId, isSameUser ? null : args.user.UserId);
        await LoadDataAsync();
    }

    private void CloseDrawer()
    {
        _isDrawerOpen = false;
        _currentForm = new TaskFormModel();
    }

    private async Task SaveTask()
    {
        if (_isEditMode)
        {
            await HouseService.UpdateTaskAsync(_currentForm);
        }
        else
        {
            await HouseService.AddTaskAsync(_currentForm);
        }
        
        await LoadDataAsync();
        CloseDrawer();
    }
    
    private async Task RemoveTask()
    {
        await HouseService.RemoveTaskAsync(_currentForm.Id);
        await LoadDataAsync();
        CloseDrawer();
    }

    private async Task ToggleTask(TaskDetailsDto task)
    {
        await HouseService.ToggleTaskAsync(task.TaskId, task.IsDone);
        await TaskCompleted(task);
        await UserService.UpdateTotalXpUser(_connectedUser.UserId);
        await LoadDataAsync();
    }
    
    private async Task TaskCompleted(TaskDetailsDto task)
    {
        var taskCompletionModel = new TaskCompletionModel()
        {
            CompletedAt = new DateTime(),
            HouseTaskId = task.TaskId,
            CompletedById = _connectedUser!.UserId,
            XpEarned = 10,
        };

        await HouseService.CreateTaskCompletionAsync(taskCompletionModel);
    }
}