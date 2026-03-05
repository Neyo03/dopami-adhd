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
        if (_connectedUser == null) { return; }
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
        var roomsTask            = HouseService.GetRoomsAsync(_connectedUser!.HomeId);
        var roomsWithTasksTask   = HouseService.GetRoomsInprogressTasksOptimizedAsync(_connectedUser.HomeId);
        var countTasksTask       = HouseService.GetTotalTasksAsync(_connectedUser.HomeId);
        var countDoneTasksTask   = HouseService.GetTotalDoneTasksAsync(_connectedUser.HomeId);
        var countAssignedTask    = HouseService.CountAssignedTasksAsync(_connectedUser.UserId);
        var doneTasksWeekTask    = HouseService.GetRoomsDoneTasksWeekAsync(_connectedUser.HomeId);
        var assignedTasksTask    = HouseService.GetAssignedTasksAsync(_connectedUser.UserId);
        var levelingTask         = LevelingService.CalculateLevelAsync(_connectedUser.TotalXp);

        await Task.WhenAll(
            roomsTask, roomsWithTasksTask, countTasksTask, countDoneTasksTask,
            countAssignedTask, doneTasksWeekTask, assignedTasksTask, levelingTask
        );

        _rooms              = roomsTask.Result;
        _roomsWithTasks     = roomsWithTasksTask.Result;
        _countTasks         = countTasksTask.Result;
        _countDoneTasks     = countDoneTasksTask.Result;
        _countAssignedTasks = countAssignedTask.Result;
        _roomsWithDoneTasks = doneTasksWeekTask.Result;
        _assignedTasks      = assignedTasksTask.Result;
        _levelingUser       = levelingTask.Result;
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
        args.task.AssignedUsername = isSameUser ? null : args.user.Username;
        await HouseService.AssignUserTaskAsync(args.task.TaskId, args.user.HomeId, isSameUser ? null : args.user.UserId);
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
            await HouseService.UpdateTaskAsync(_currentForm, _connectedUser!.HomeId);
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
        await HouseService.RemoveTaskAsync(_currentForm.Id, _connectedUser!.HomeId);
        await LoadDataAsync();
        CloseDrawer();
    }

    private async Task ToggleTask(TaskDetailsDto task)
    {
        await HouseService.ToggleTaskAsync(task.TaskId, task.IsDone, _connectedUser!.HomeId);
        if (!task.IsDone) await TaskCompleted(task);
        await UserService.UpdateTotalXpUser(_connectedUser.UserId);
        await LoadDataAsync();
    }
    
    private async Task TaskCompleted(TaskDetailsDto task)
    {
        var taskCompletionModel = new TaskCompletionModel()
        {
            CompletedAt = DateTime.UtcNow,
            HouseTaskId = task.TaskId,
            CompletedById = _connectedUser!.UserId,
            XpEarned = 10,
        };

        await HouseService.CreateTaskCompletionAsync(taskCompletionModel);
    }
}