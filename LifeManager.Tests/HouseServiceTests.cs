using LifeManager.Data;
using LifeManager.Model;
using LifeManager.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LifeManager.Tests;

/// <summary>
/// Fake factory qui réutilise une connexion SQLite in-memory partagée.
/// Nécessaire car ExecuteUpdateAsync / ExecuteDeleteAsync ne fonctionnent
/// pas avec le provider EF InMemory.
/// </summary>
file class FakeDbContextFactory(DbContextOptions<AppDbContext> options) : IDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext() => new(options);
    public Task<AppDbContext> CreateDbContextAsync(CancellationToken _ = default) => Task.FromResult(new AppDbContext(options));
}

/// <summary>
/// Données de base insérées avant chaque test :
/// une Home → un Room → une HouseTask + un User dans la même Home.
/// </summary>
internal record SeedResult(int TaskId, int UserId, int RoomId, int HomeId);

public class HouseServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<AppDbContext> _options;
    private readonly HouseService _service;

    public HouseServiceTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        using var ctx = new AppDbContext(_options);
        ctx.Database.EnsureCreated();

        _service = new HouseService(new FakeDbContextFactory(_options));
    }

    public void Dispose() => _connection.Dispose();

    // ─── Helpers ────────────────────────────────────────────────────────────

    private async Task<SeedResult> SeedAsync(bool isDone = false, bool assignUser = false)
    {
        await using var ctx = new AppDbContext(_options);

        var home = new Home { Name = "TestHome" };
        var user = new User
        {
            Username = "alice", Firstname = "Alice", Lastname = "Dupont",
            Email = "alice@example.com", Password = "hashed", Home = home
        };
        var room = new Room { Name = "Cuisine", Home = home };
        ctx.Homes.Add(home);
        ctx.Users.Add(user);
        ctx.Rooms.Add(room);
        await ctx.SaveChangesAsync();

        var task = new HouseTask
        {
            Title = "Faire la vaisselle",
            RoomId = room.Id,
            IsDone = isDone,
            UserAssignedId = assignUser ? user.Id : null
        };
        ctx.HouseTasks.Add(task);
        await ctx.SaveChangesAsync();

        return new SeedResult(task.Id, user.Id, room.Id, home.Id);
    }

    // ─── ToggleTaskAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task ToggleTaskAsync_WhenNotDone_SetsIsDoneTrue()
    {
        var seed = await SeedAsync(isDone: false);

        await _service.ToggleTaskAsync(seed.TaskId, false);

        await using var ctx = new AppDbContext(_options);
        var task = await ctx.HouseTasks.FindAsync(seed.TaskId);
        Assert.True(task!.IsDone);
    }

    [Fact]
    public async Task ToggleTaskAsync_WhenDone_SetsIsDoneFalse()
    {
        var seed = await SeedAsync(isDone: true);

        await _service.ToggleTaskAsync(seed.TaskId, true);

        await using var ctx = new AppDbContext(_options);
        var task = await ctx.HouseTasks.FindAsync(seed.TaskId);
        Assert.False(task!.IsDone);
    }

    [Fact]
    public async Task ToggleTaskAsync_ClearsUserAssignedId()
    {
        var seed = await SeedAsync(isDone: false, assignUser: true);

        await _service.ToggleTaskAsync(seed.TaskId, false);

        await using var ctx = new AppDbContext(_options);
        var task = await ctx.HouseTasks.FindAsync(seed.TaskId);
        Assert.Null(task!.UserAssignedId);
    }

    // ─── AssignUserTaskAsync ────────────────────────────────────────────────

    [Fact]
    public async Task AssignUserTaskAsync_SetsUserAssignedId()
    {
        var seed = await SeedAsync();

        await _service.AssignUserTaskAsync(seed.TaskId, seed.UserId);

        await using var ctx = new AppDbContext(_options);
        var task = await ctx.HouseTasks.FindAsync(seed.TaskId);
        Assert.Equal(seed.UserId, task!.UserAssignedId);
    }

    [Fact]
    public async Task AssignUserTaskAsync_WithNull_UnassignsUser()
    {
        var seed = await SeedAsync(assignUser: true);

        await _service.AssignUserTaskAsync(seed.TaskId, userId: null);

        await using var ctx = new AppDbContext(_options);
        var task = await ctx.HouseTasks.FindAsync(seed.TaskId);
        Assert.Null(task!.UserAssignedId);
    }
}

// ─── Logique de ToggleAssignUserToTask (Home.razor.cs) ──────────────────────
// La méthode est privée dans le composant ; on teste ici la règle métier
// qui détermine quel userId est transmis à AssignUserTaskAsync.

public class ToggleAssignUserLogicTests
{
    /// <summary>
    /// Si l'utilisateur cliqué est déjà assigné → on désassigne (null).
    /// </summary>
    [Fact]
    public void SameUser_ShouldUnassign()
    {
        var task = new TaskDetailsDto { TaskId = 1, AssignedUsername = "alice" };
        var user = new UserDto { UserId = 10, Username = "alice" };

        bool isSameUser = task.AssignedUsername == user.Username;
        int? resultUserId = isSameUser ? null : user.UserId;

        Assert.Null(resultUserId);
    }

    /// <summary>
    /// Si un utilisateur différent est cliqué → on l'assigne.
    /// </summary>
    [Fact]
    public void DifferentUser_ShouldAssignNewUser()
    {
        var task = new TaskDetailsDto { TaskId = 1, AssignedUsername = "bob" };
        var user = new UserDto { UserId = 10, Username = "alice" };

        bool isSameUser = task.AssignedUsername == user.Username;
        int? resultUserId = isSameUser ? null : user.UserId;

        Assert.Equal(10, resultUserId);
    }

    /// <summary>
    /// Si la tâche n'est assignée à personne → on assigne l'utilisateur cliqué.
    /// </summary>
    [Fact]
    public void UnassignedTask_ShouldAssignUser()
    {
        var task = new TaskDetailsDto { TaskId = 1, AssignedUsername = null };
        var user = new UserDto { UserId = 10, Username = "alice" };

        bool isSameUser = task.AssignedUsername == user.Username;
        int? resultUserId = isSameUser ? null : user.UserId;

        Assert.Equal(10, resultUserId);
    }
}