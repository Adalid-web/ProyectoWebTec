using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoTecWeb.Data;
using ProyectoTecWeb.DTOs;
using ProyectoTecWeb.Models;

namespace ProyectoTecWeb.Controllers;

[ApiController]
[Authorize]
[Route("api/argentina/logros")]
[Tags("Argentina · Logros")]
public class LogrosArgentinaController : ControllerBase
{
    private readonly AppDbContext _db;
    public LogrosArgentinaController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<LogroArgentinaResponseDto>>> GetAll()
    {
        var logros = await _db.LogrosArgentina.AsNoTracking()
            .OrderByDescending(l => l.FechaOtorgado)
            .Select(l => new LogroArgentinaResponseDto
            {
                Id = l.Id, JugadorId = l.JugadorId, Nombre = l.Nombre,
                Descripcion = l.Descripcion, FechaOtorgado = l.FechaOtorgado
            }).ToListAsync();
        return Ok(logros);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<LogroArgentinaResponseDto>> GetById(int id)
    {
        var l = await _db.LogrosArgentina.AsNoTracking()
            .Where(l => l.Id == id)
            .Select(l => new LogroArgentinaResponseDto
            {
                Id = l.Id, JugadorId = l.JugadorId, Nombre = l.Nombre,
                Descripcion = l.Descripcion, FechaOtorgado = l.FechaOtorgado
            }).FirstOrDefaultAsync();
        if (l is null) return NotFound(new { message = "Logro no encontrado." });
        return Ok(l);
    }

    [HttpPost]
    public async Task<ActionResult<LogroArgentinaResponseDto>> Create([FromBody] LogroArgentinaRequestDto dto)
    {
        if (!await _db.JugadoresArgentina.AnyAsync(j => j.Id == dto.JugadorId))
            return NotFound(new { message = "El jugador indicado no existe." });
        if (string.IsNullOrWhiteSpace(dto.Nombre))
            return BadRequest(new { message = "El nombre del logro es obligatorio." });

        var l = new LogroArgentina
        {
            JugadorId = dto.JugadorId, Nombre = dto.Nombre.Trim(),
            Descripcion = string.IsNullOrWhiteSpace(dto.Descripcion) ? null : dto.Descripcion.Trim(),
            FechaOtorgado = DateTime.UtcNow
        };
        _db.LogrosArgentina.Add(l);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = l.Id }, new LogroArgentinaResponseDto
        {
            Id = l.Id, JugadorId = l.JugadorId, Nombre = l.Nombre,
            Descripcion = l.Descripcion, FechaOtorgado = l.FechaOtorgado
        });
    }
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] LogroArgentinaRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre))
            return BadRequest(new { message = "El nombre del logro es obligatorio." });
        var logro = await _db.LogrosArgentina.FirstOrDefaultAsync(l => l.Id == id);
        if (logro is null) return NotFound(new { message = "Logro no encontrado." });
        if (!await _db.JugadoresArgentina.AnyAsync(j => j.Id == dto.JugadorId))
            return NotFound(new { message = "El jugador indicado no existe." });

        logro.JugadorId = dto.JugadorId;
        logro.Nombre = dto.Nombre.Trim();
        logro.Descripcion = string.IsNullOrWhiteSpace(dto.Descripcion) ? null : dto.Descripcion.Trim();
        await _db.SaveChangesAsync();
        return NoContent();
    }

}
