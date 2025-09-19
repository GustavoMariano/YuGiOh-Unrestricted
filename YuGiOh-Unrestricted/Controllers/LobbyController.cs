using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YuGiOh_Unrestricted.Core.Models;
using YuGiOh_Unrestricted.Infrastructure.Data;

namespace YuGiOh_Unrestricted.Controllers;

[Route("lobby")]
public class LobbyController : Controller
{
    private readonly GameDbContext _db;
    private readonly AppState _app;

    public LobbyController(GameDbContext db, AppState app)
    {
        _db = db;
        _app = app;
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create()
    {
        var code = Guid.NewGuid().ToString("N")[..6].ToUpper();

        var match = new Match
        {
            Code = code,
            Players = new List<MatchPlayer>(),
            Turn = 1
        };

        _db.Matches.Add(match);
        await _db.SaveChangesAsync();

        var host = new MatchPlayer
        {
            MatchId = match.Id,
            UserId = _app.UserId,
            Nickname = _app.Nickname
        };

        _db.MatchPlayers.Add(host);
        await _db.SaveChangesAsync();

        return Redirect($"/battle/{match.Code}");
    }

    [HttpPost("join")]
    public async Task<IActionResult> Join()
    {
        var form = await Request.ReadFormAsync();
        var code = form["code"].ToString().Trim().ToUpper();

        if (string.IsNullOrWhiteSpace(code))
            return BadRequest("Invalid battle code.");

        var match = await _db.Matches
            .Include(m => m.Players)
            .FirstOrDefaultAsync(m => m.Code == code);

        if (match == null)
            return NotFound("Battle not found.");

        if (match.Players.Count >= 2)
            return BadRequest("Battle is already full.");

        var player = new MatchPlayer
        {
            MatchId = match.Id,
            UserId = _app.UserId,
            Nickname = _app.Nickname
        };

        _db.MatchPlayers.Add(player);
        await _db.SaveChangesAsync();

        return Redirect($"/battle/{match.Code}");
    }
}
