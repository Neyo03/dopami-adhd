using LifeManager.Data;
using LifeManager.Model;
using LifeManager.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LifeManager.Tests;

file class FakeDbContextFactory2(DbContextOptions<AppDbContext> options) : IDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext() => new(options);
    public Task<AppDbContext> CreateDbContextAsync(CancellationToken _ = default) => Task.FromResult(new AppDbContext(options));
}

file class FakeHttpContextAccessor : IHttpContextAccessor
{
    public HttpContext? HttpContext { get; set; } = null;
}

public class TaskCompletionTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<AppDbContext> _options;
    private readonly HouseService _houseService;
    private readonly UserService _userService;

    public TaskCompletionTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        using var ctx = new AppDbContext(_options);
        ctx.Database.EnsureCreated();

        var factory = new FakeDbContextFactory2(_options);
        _houseService = new HouseService(factory);
        _userService = new UserService(factory, new FakeHttpContextAccessor());
    }

    public void Dispose() => _connection.Dispose();

    // ─── Helpers ────────────────────────────────────────────────────────────

    private async Task<SeedResult> SeedAsync()
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

        var task = new HouseTask { Title = "Faire la vaisselle", RoomId = room.Id };
        ctx.HouseTasks.Add(task);
        await ctx.SaveChangesAsync();

        return new SeedResult(task.Id, user.Id, room.Id, home.Id);
    }

    private async Task InsertCompletionAsync(int taskId, int userId, DateTime completedAt, int xp = 10)
    {
        await using var ctx = new AppDbContext(_options);
        ctx.TaskCompletions.Add(new TaskCompletion
        {
            HouseTaskId = taskId,
            CompletedById = userId,
            CompletedAt = completedAt,
            XpEarned = xp
        });
        await ctx.SaveChangesAsync();
    }

    // ─── CreateTaskCompletionAsync ───────────────────────────────────────────

    [Fact]
    public async Task CreateTaskCompletionAsync_PersistsAllFields()
    {
        var seed = await SeedAsync();
        var completedAt = DateTime.UtcNow;

        await _houseService.CreateTaskCompletionAsync(new TaskCompletionModel
        {
            HouseTaskId = seed.TaskId,
            CompletedById = seed.UserId,
            CompletedAt = completedAt,
            XpEarned = 15
        });

        await using var ctx = new AppDbContext(_options);
        var record = await ctx.TaskCompletions.SingleAsync();
        Assert.Equal(seed.TaskId, record.HouseTaskId);
        Assert.Equal(seed.UserId, record.CompletedById);
        Assert.Equal(15, record.XpEarned);
    }

    [Fact]
    public async Task CreateTaskCompletionAsync_DoesNotStoreInfinityDate()
    {
        var seed = await SeedAsync();

        // Simule le comportement correct (DateTime.UtcNow)
        await _houseService.CreateTaskCompletionAsync(new TaskCompletionModel
        {
            HouseTaskId = seed.TaskId,
            CompletedById = seed.UserId,
            CompletedAt = DateTime.UtcNow,
            XpEarned = 10
        });

        await using var ctx = new AppDbContext(_options);
        var record = await ctx.TaskCompletions.SingleAsync();
        Assert.NotEqual(DateTime.MinValue, record.CompletedAt);
        Assert.NotEqual(DateTime.MaxValue, record.CompletedAt);
    }

    [Fact]
    public async Task CreateTaskCompletionAsync_MultipleCompletions_AllPersisted()
    {
        var seed = await SeedAsync();

        await _houseService.CreateTaskCompletionAsync(new TaskCompletionModel
        {
            HouseTaskId = seed.TaskId, CompletedById = seed.UserId,
            CompletedAt = DateTime.UtcNow, XpEarned = 10
        });
        await _houseService.CreateTaskCompletionAsync(new TaskCompletionModel
        {
            HouseTaskId = seed.TaskId, CompletedById = seed.UserId,
            CompletedAt = DateTime.UtcNow, XpEarned = 20
        });

        await using var ctx = new AppDbContext(_options);
        Assert.Equal(2, await ctx.TaskCompletions.CountAsync());
    }

    // ─── UpdateTotalXpUser ────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateTotalXpUser_SumsAllCompletions()
    {
        var seed = await SeedAsync();
        await InsertCompletionAsync(seed.TaskId, seed.UserId, DateTime.UtcNow, xp: 10);
        await InsertCompletionAsync(seed.TaskId, seed.UserId, DateTime.UtcNow, xp: 15);

        await _userService.UpdateTotalXpUser(seed.UserId);

        await using var ctx = new AppDbContext(_options);
        var user = await ctx.Users.FindAsync(seed.UserId);
        Assert.Equal(25, user!.TotalXp);
    }

    [Fact]
    public async Task UpdateTotalXpUser_WithNoCompletions_SetsZero()
    {
        var seed = await SeedAsync();

        await _userService.UpdateTotalXpUser(seed.UserId);

        await using var ctx = new AppDbContext(_options);
        var user = await ctx.Users.FindAsync(seed.UserId);
        Assert.Equal(0, user!.TotalXp);
    }

    [Fact]
    public async Task UpdateTotalXpUser_OnlyCountsCompletionsForThatUser()
    {
        var seed = await SeedAsync();

        // Ajoute un 2ème user dans la même home
        await using var ctx2 = new AppDbContext(_options);
        var otherUser = new User
        {
            Username = "bob", Firstname = "Bob", Lastname = "Martin",
            Email = "bob@example.com", Password = "hashed", HomeId = seed.HomeId
        };
        ctx2.Users.Add(otherUser);
        await ctx2.SaveChangesAsync();

        await InsertCompletionAsync(seed.TaskId, seed.UserId, DateTime.UtcNow, xp: 10);
        await InsertCompletionAsync(seed.TaskId, otherUser.Id, DateTime.UtcNow, xp: 50);

        await _userService.UpdateTotalXpUser(seed.UserId);

        await using var ctx = new AppDbContext(_options);
        var user = await ctx.Users.FindAsync(seed.UserId);
        Assert.Equal(10, user!.TotalXp); // Ne doit pas inclure les 50 XP de bob
    }

    // ─── GetRoomsDoneTasksWeekAsync ──────────────────────────────────────────

    [Fact]
    public async Task GetRoomsDoneTasksWeek_ReturnsCompletionsFromThisWeek()
    {
        var seed = await SeedAsync();
        await InsertCompletionAsync(seed.TaskId, seed.UserId, DateTime.UtcNow, xp: 10);

        var result = await _houseService.GetRoomsDoneTasksWeekAsync(seed.HomeId);

        Assert.Single(result);
        Assert.Equal("alice", result[0].Username);
    }

    [Fact]
    public async Task GetRoomsDoneTasksWeek_ExcludesCompletionsBeforeThisWeek()
    {
        var seed = await SeedAsync();
        var lastWeek = DateTime.UtcNow.AddDays(-8);
        await InsertCompletionAsync(seed.TaskId, seed.UserId, lastWeek, xp: 10);

        var result = await _houseService.GetRoomsDoneTasksWeekAsync(seed.HomeId);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetRoomsDoneTasksWeek_GroupsCompletionsByDateAndUser()
    {
        var seed = await SeedAsync();
        var today = DateTime.UtcNow;

        // 2 completions aujourd'hui pour alice → 1 seul groupe
        await InsertCompletionAsync(seed.TaskId, seed.UserId, today, xp: 10);
        await InsertCompletionAsync(seed.TaskId, seed.UserId, today, xp: 15);

        var result = await _houseService.GetRoomsDoneTasksWeekAsync(seed.HomeId);

        Assert.Single(result);
        Assert.Equal(2, result[0].Tasks.Count);
    }

    [Fact]
    public async Task GetRoomsDoneTasksWeek_SeparatesGroupsByUser()
    {
        var seed = await SeedAsync();

        await using var ctx2 = new AppDbContext(_options);
        var bob = new User
        {
            Username = "bob", Firstname = "Bob", Lastname = "Martin",
            Email = "bob@example.com", Password = "hashed", HomeId = seed.HomeId
        };
        ctx2.Users.Add(bob);
        await ctx2.SaveChangesAsync();

        var today = DateTime.UtcNow;
        await InsertCompletionAsync(seed.TaskId, seed.UserId, today, xp: 10);
        await InsertCompletionAsync(seed.TaskId, bob.Id, today, xp: 20);

        var result = await _houseService.GetRoomsDoneTasksWeekAsync(seed.HomeId);

        Assert.Equal(2, result.Count);
        Assert.Contains(result, g => g.Username == "alice");
        Assert.Contains(result, g => g.Username == "bob");
    }
}