using Microsoft.EntityFrameworkCore;
using YuGiOh_Unrestricted.Hubs;
using YuGiOh_Unrestricted.Infrastructure.Data;
using YuGiOh_Unrestricted.Infrastructure.Services.Implementations;
using YuGiOh_Unrestricted.Infrastructure.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

builder.Services.AddDbContext<GameDbContext>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("Default") ?? "Data Source=game.db"));

builder.Services.AddScoped<ICardService, CardService>();
builder.Services.AddScoped<IDeckService, DeckService>();

builder.Services.AddSingleton<AppState>();

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.MapHub<GameHub>("/gamehub");

app.MapPost("/deckbuilder/{deckId:guid}/add", async (Guid deckId, HttpRequest req, IDeckService decksSvc) =>
{
    var form = await req.ReadFormAsync();
    if (Guid.TryParse(form["cardId"], out var cardId))
    {
        await decksSvc.AddCopyAsync(deckId, cardId);
    }
    return Results.Redirect($"/deckbuilder/{deckId}");
});

app.MapPost("/deckbuilder/{deckId:guid}/remove", async (Guid deckId, HttpRequest req, IDeckService decksSvc) =>
{
    var form = await req.ReadFormAsync();
    if (Guid.TryParse(form["cardId"], out var cardId))
    {
        await decksSvc.RemoveCopyAsync(deckId, cardId);
    }
    return Results.Redirect($"/deckbuilder/{deckId}");
});

app.MapPost("/deckbuilder/{deckId:guid}/rename", async (Guid deckId, HttpRequest req, IDeckService decksSvc) =>
{
    var form = await req.ReadFormAsync();
    var name = form["name"].ToString() ?? "";
    if (!string.IsNullOrWhiteSpace(name))
    {
        await decksSvc.RenameDeckAsync(deckId, name);
    }
    return Results.Redirect($"/deckbuilder/{deckId}");
});

app.Run();

public class AppState
{
    public string Nickname { get; set; } = "";
    public Guid UserId { get; set; } = Guid.Empty;
}
