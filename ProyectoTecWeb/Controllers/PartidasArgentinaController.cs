using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoTecWeb.Data;
using ProyectoTecWeb.DTOs;
using ProyectoTecWeb.Models;

namespace ProyectoTecWeb.Controllers;

[ApiController]
[Authorize]
[Route("api/argentina/partidas")]
[Tags("Argentina · Partidas")]
public class PartidasArgentinaController : ControllerBase
{
    private readonly AppDbContext _db;
    public PartidasArgentinaController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PartidaArgentinaResponseDto>>> GetAll()
    {
        var partidas = await _db.PartidasArgentina.AsNoTracking()
            .OrderByDescending(p => p.JugadoEn)
            .Select(p => new PartidaArgentinaResponseDto
            {
                Id = p.Id, JugadorId = p.JugadorId, Puntaje = p.Puntaje,
                DuracionSegundos = p.DuracionSegundos, JugadoEn = p.JugadoEn
            }).ToListAsync();
        return Ok(partidas);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PartidaArgentinaResponseDto>> GetById(int id)
    {
        var p = await _db.PartidasArgentina.AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new PartidaArgentinaResponseDto
            {
                Id = p.Id, JugadorId = p.JugadorId, Puntaje = p.Puntaje,
                DuracionSegundos = p.DuracionSegundos, JugadoEn = p.JugadoEn
            }).FirstOrDefaultAsync();
        if (p is null) return NotFound(new { message = "Partida no encontrada." });
        return Ok(p);
    }

    [HttpPost]
    public async Task<ActionResult<PartidaArgentinaResponseDto>> Create([FromBody] PartidaArgentinaRequestDto dto)
    {
        if (!await _db.JugadoresArgentina.AnyAsync(j => j.Id == dto.JugadorId))
            return NotFound(new { message = "El jugador indicado no existe." });

        var p = new PartidaArgentina
        {
            JugadorId = dto.JugadorId, Puntaje = dto.Puntaje,
            DuracionSegundos = dto.DuracionSegundos, JugadoEn = DateTime.UtcNow
        };
        _db.PartidasArgentina.Add(p);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = p.Id }, new PartidaArgentinaResponseDto
        {
            Id = p.Id, JugadorId = p.JugadorId, Puntaje = p.Puntaje,
            DuracionSegundos = p.DuracionSegundos, JugadoEn = p.JugadoEn
        });
    }
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] PartidaArgentinaRequestDto dto)
    {
        var partida = await _db.PartidasArgentina.FirstOrDefaultAsync(p => p.Id == id);
        if (partida is null) return NotFound(new { message = "Partida no encontrada." });
        if (!await _db.JugadoresArgentina.AnyAsync(j => j.Id == dto.JugadorId))
            return NotFound(new { message = "El jugador indicado no existe." });

        partida.JugadorId = dto.JugadorId;
        partida.Puntaje = dto.Puntaje;
        partida.DuracionSegundos = dto.DuracionSegundos;
        await _db.SaveChangesAsync();
        return NoContent();
    }

}
