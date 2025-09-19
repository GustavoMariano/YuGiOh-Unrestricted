using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using YuGiOh_Unrestricted.Hubs;
using YuGiOh_Unrestricted.Hubs.Runtime;
using YuGiOh_Unrestricted.Infrastructure.Data;
using YuGiOh_Unrestricted.Infrastructure.Services.Implementations;
using YuGiOh_Unrestricted.Infrastructure.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();

builder.Services.AddDbContext<GameDbContext>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("Default") ?? "Data Source=game.db"));

builder.Services.AddScoped<ICardService, CardService>();
builder.Services.AddScoped<IDeckService, DeckService>();

builder.Services.AddSingleton<IMatchRuntime, MatchRuntime>();
builder.Services.AddScoped<AppState>();

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();

app.MapControllers();
app.MapBlazorHub();
app.MapHub<GameHub>("/gamehub");

app.MapPost("/deckbuilder/{deckId:guid}/add", async (Guid deckId, HttpRequest req, IDeckService decksSvc) =>
{
    var form = await req.ReadFormAsync();
    if (Guid.TryParse(form["cardId"], out var cardId))
        await decksSvc.AddCopyAsync(deckId, cardId);
    return Results.Redirect($"/deckbuilder/{deckId}");
});

app.MapPost("/deckbuilder/{deckId:guid}/remove", async (Guid deckId, HttpRequest req, IDeckService decksSvc) =>
{
    var form = await req.ReadFormAsync();
    if (Guid.TryParse(form["cardId"], out var cardId))
        await decksSvc.RemoveCopyAsync(deckId, cardId);
    return Results.Redirect($"/deckbuilder/{deckId}");
});

app.MapPost("/deckbuilder/{deckId:guid}/rename", async (Guid deckId, HttpRequest req, IDeckService decksSvc) =>
{
    var form = await req.ReadFormAsync();
    var name = form["name"].ToString() ?? "";
    if (!string.IsNullOrWhiteSpace(name))
        await decksSvc.RenameDeckAsync(deckId, name);
    return Results.Redirect($"/deckbuilder/{deckId}");
});

app.MapPost("/battle/{code}/select-deck", async (string code, HttpRequest req, IMatchRuntime runtime) =>
{
    var form = await req.ReadFormAsync();
    var deckIdStr = form["deckId"].ToString();
    var userIdStr = form["userId"].ToString();
    if (Guid.TryParse(deckIdStr, out var deckId) && Guid.TryParse(userIdStr, out var userId))
        await runtime.SelectDeckAsync(code, userId, deckId);
    return Results.NoContent();
});

app.MapPost("/battle/{code}/ready", async (string code, HttpRequest req, IMatchRuntime runtime) =>
{
    var form = await req.ReadFormAsync();
    var userIdStr = form["userId"].ToString();
    if (Guid.TryParse(userIdStr, out var userId))
        await runtime.SetReadyAsync(code, userId);
    return Results.NoContent();
});

app.MapPost("/battle/{code}/draw", async (string code, HttpRequest req, IMatchRuntime runtime) =>
{
    var form = await req.ReadFormAsync();
    var userIdStr = form["userId"].ToString();
    if (Guid.TryParse(userIdStr, out var userId))
        await runtime.DrawOneAsync(code, userId);
    return Results.NoContent();
});

app.MapPost("/auth/login", async (HttpRequest req, GameDbContext db) =>
{
    var form = await req.ReadFormAsync();
    var nickname = form["nickname"].ToString();
    var password = form["password"].ToString();

    if (string.IsNullOrWhiteSpace(nickname) || string.IsNullOrWhiteSpace(password))
        return Results.Redirect("/login?error=1");

    var user = await db.Users.FirstOrDefaultAsync(u => u.Nickname == nickname);
    if (user == null)
    {
        user = new YuGiOh_Unrestricted.Core.Models.User
        {
            Nickname = nickname,
            PasswordHash = password
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
    }
    else
    {
        if (user.PasswordHash != password)
            return Results.Redirect("/login?error=1");
    }

    var uid = user.Id.ToString();
    var cookieOptions = new CookieOptions { HttpOnly = true, IsEssential = true, Expires = DateTimeOffset.UtcNow.AddDays(7) };
    req.HttpContext.Response.Cookies.Append("uid", uid, cookieOptions);
    req.HttpContext.Response.Cookies.Append("nick", user.Nickname, cookieOptions);

    return Results.Redirect("/lobby");
});

app.MapPost("/auth/logout", (HttpRequest req) =>
{
    req.HttpContext.Response.Cookies.Delete("uid");
    req.HttpContext.Response.Cookies.Delete("nick");
    return Results.Redirect("/login");
});

app.MapGet("/auth/me", (HttpRequest req) =>
{
    var uid = req.Cookies["uid"];
    var nick = req.Cookies["nick"];
    if (Guid.TryParse(uid, out var userId) && !string.IsNullOrWhiteSpace(nick))
    {
        var payload = JsonSerializer.Serialize(new { userId, nickname = nick });
        return Results.Text(payload, "application/json");
    }
    return Results.NoContent();
});

app.MapFallbackToPage("/_Host");

app.Run();

public class AppState
{
    public string Nickname { get; set; } = "";
    public Guid UserId { get; set; } = Guid.Empty;
}
