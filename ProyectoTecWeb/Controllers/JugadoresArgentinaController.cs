using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoTecWeb.Data;
using ProyectoTecWeb.DTOs;
using ProyectoTecWeb.Models;

namespace ProyectoTecWeb.Controllers;

[ApiController]
[Authorize]
[Route("api/argentina/jugadores")]
[Tags("Argentina · Jugadores")]
public class JugadoresArgentinaController : ControllerBase
{
    private readonly AppDbContext _db;
    public JugadoresArgentinaController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<JugadorArgentinaResponseDto>>> GetAll()
    {
        var jugadores = await _db.JugadoresArgentina.AsNoTracking()
            .OrderBy(j => j.Id)
            .Select(j => new JugadorArgentinaResponseDto
            {
                Id = j.Id, Nombre = j.Nombre, Alias = j.Alias,
                FechaRegistro = j.FechaRegistro
            }).ToListAsync();
        return Ok(jugadores);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<JugadorArgentinaResponseDto>> GetById(int id)
    {
        var j = await _db.JugadoresArgentina.AsNoTracking()
            .Where(j => j.Id == id)
            .Select(j => new JugadorArgentinaResponseDto
            {
                Id = j.Id, Nombre = j.Nombre, Alias = j.Alias,
                FechaRegistro = j.FechaRegistro
            }).FirstOrDefaultAsync();
        if (j is null) return NotFound(new { message = "Jugador no encontrado." });
        return Ok(j);
    }

    [HttpPost]
    public async Task<ActionResult<JugadorArgentinaResponseDto>> Create([FromBody] JugadorArgentinaRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre) || string.IsNullOrWhiteSpace(dto.Alias))
            return BadRequest(new { message = "El nombre y el alias son obligatorios." });

        var nombre = dto.Nombre.Trim();
        var alias = dto.Alias.Trim();
        if (await _db.JugadoresArgentina.AnyAsync(j => j.Alias == alias))
            return Conflict(new { message = "El alias ya existe." });

        var j = new JugadorArgentina
        {
            Nombre = nombre, Alias = alias, FechaRegistro = DateTime.UtcNow
        };
        _db.JugadoresArgentina.Add(j);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = j.Id }, new JugadorArgentinaResponseDto
        {
            Id = j.Id, Nombre = j.Nombre, Alias = j.Alias,
            FechaRegistro = j.FechaRegistro
        });
    }
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] JugadorArgentinaRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre) || string.IsNullOrWhiteSpace(dto.Alias))
            return BadRequest(new { message = "El nombre y el alias son obligatorios." });
        var jugador = await _db.JugadoresArgentina.FirstOrDefaultAsync(j => j.Id == id);
        if (jugador is null) return NotFound(new { message = "Jugador no encontrado." });

        var alias = dto.Alias.Trim();
        if (await _db.JugadoresArgentina.AnyAsync(j => j.Id != id && j.Alias == alias))
            return Conflict(new { message = "El alias ya existe." });

        jugador.Nombre = dto.Nombre.Trim();
        jugador.Alias = alias;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var jugador = await _db.JugadoresArgentina.FirstOrDefaultAsync(x => x.Id == id);
        if (jugador is null) return NotFound(new { message = "Jugador no encontrado." });

        jugador.IsDeleted = true;
        jugador.DeletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return NoContent();
    }

}
