using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoTecWeb.Data;
using ProyectoTecWeb.DTOs;
using ProyectoTecWeb.Models;

namespace ProyectoTecWeb.Controllers;

[ApiController]
[Authorize]
[Route("api/leaderboard/argentina")]
[Tags("Leaderboard Argentina")]
public class LeaderboardArgentinaController : ControllerBase
{
    private readonly AppDbContext _db;
    public LeaderboardArgentinaController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ScoreResponseDto>>> GetAll([FromQuery] int limit = 100)
    {
        limit = Math.Clamp(limit, 1, 500);
        var scores = await _db.ArgentinaScores.AsNoTracking()
            .OrderByDescending(s => s.Score).ThenBy(s => s.CreatedAt)
            .Take(limit)
            .Select(s => new ScoreResponseDto
            {
                Id = s.Id, PlayerName = s.PlayerName, Score = s.Score, CreatedAt = s.CreatedAt
            }).ToListAsync();
        return Ok(scores);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ScoreResponseDto>> GetById(int id)
    {
        var score = await _db.ArgentinaScores.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);
        if (score is null) return NotFound(new { message = "Puntaje no encontrado." });
        return Ok(ToResponse(score));
    }

    [HttpPost]
    public async Task<ActionResult<ScoreResponseDto>> Create([FromBody] ScoreRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.PlayerName))
            return BadRequest(new { message = "El nombre no puede estar vacío." });

        var score = new ScoreArgentina
        {
            PlayerName = dto.PlayerName.Trim(), Score = dto.Score,
            CreatedAt = DateTime.UtcNow
        };
        _db.ArgentinaScores.Add(score);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = score.Id }, ToResponse(score));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] ScoreRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.PlayerName))
            return BadRequest(new { message = "El nombre no puede estar vacío." });
        var score = await _db.ArgentinaScores.FirstOrDefaultAsync(s => s.Id == id);
        if (score is null) return NotFound(new { message = "Puntaje no encontrado." });

        score.PlayerName = dto.PlayerName.Trim();
        score.Score = dto.Score;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var score = await _db.ArgentinaScores.FirstOrDefaultAsync(x => x.Id == id);
        if (score is null) return NotFound(new { message = "Puntaje no encontrado." });

        score.IsDeleted = true;
        score.DeletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private static ScoreResponseDto ToResponse(ScoreArgentina s) => new()
    {
        Id = s.Id, PlayerName = s.PlayerName, Score = s.Score, CreatedAt = s.CreatedAt
    };
}
