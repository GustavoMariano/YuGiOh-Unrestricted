using Microsoft.EntityFrameworkCore;
using YuGiOh_Unrestricted.Hubs;
using YuGiOh_Unrestricted.Infrastructure.Data;
using YuGiOh_Unrestricted.Infrastructure.Services.Implemetations;
using YuGiOh_Unrestricted.Infrastructure.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// DbContext (SQLite)
builder.Services.AddDbContext<GameDbContext>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("Default") ?? "Data Source=game.db"));

// Services (infrastructure)
builder.Services.AddScoped<ICardService, CardService>();
builder.Services.AddScoped<IDeckService, DeckService>();

// Estado simples do usuário (nickname)
builder.Services.AddSingleton<AppState>();

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

// SignalR Hub
app.MapHub<GameHub>("/gamehub");

app.Run();

// Estado da UI (nickname)
public class AppState
{
    public string Nickname { get; set; } = "";
    public Guid UserId { get; set; } = Guid.Empty;
}
