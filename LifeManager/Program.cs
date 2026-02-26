using LifeManager.Components;
using Microsoft.EntityFrameworkCore;
using LifeManager.Data;
using LifeManager.Services;
using LifeManager.State;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    ;


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
                       ?? throw new InvalidOperationException("Chaîne de connexion 'DefaultConnection' introuvable.");

builder.Services.AddAuthentication("CookieAuth")
    .AddCookie("CookieAuth", options =>
    {
        options.LoginPath = "/login";
    });
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<HouseService>();
builder.Services.AddScoped<LevelingService>();
builder.Services.AddScoped<TagService>();
builder.Services.AddScoped<TagStateService>();
builder.Services.AddScoped<TagModalStateService>();


var app = builder.Build();
app.UseHttpsRedirection();

app.UseRouting();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();