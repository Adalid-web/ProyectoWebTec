using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoTecWeb.Data;

namespace ProyectoTecWeb.Controllers;

/// <summary>
/// Proyecto Final - reporte de Argentina. Autor: Adalid Callizaya.
/// Combina las cuatro tablas de Argentina y respeta los filtros de soft delete.
/// </summary>
[ApiController]
[Authorize(Policy = "ReporteArgentina")]
[Route("api/reportes/argentina")]
[Tags("Reportes - Argentina")]
public sealed class ReportesArgentinaController : ControllerBase
{
    private readonly AppDbContext _db;

    public ReportesArgentinaController(AppDbContext db) => _db = db;

    [HttpGet("resumen")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Resumen(CancellationToken cancellationToken)
    {
        // EF Core aplica IsDeleted = false a las cuatro consultas.
        var jugadores = await _db.JugadoresArgentina.AsNoTracking()
            .Select(j => new { j.Id, j.Nombre, j.Alias })
            .ToListAsync(cancellationToken);

        var partidas = await _db.PartidasArgentina.AsNoTracking()
            .Select(p => new { p.JugadorId, p.Puntaje, p.DuracionSegundos })
            .ToListAsync(cancellationToken);

        var logros = await _db.LogrosArgentina.AsNoTracking()
            .Select(l => new { l.JugadorId, l.Nombre })
            .ToListAsync(cancellationToken);

        var ranking = await _db.ArgentinaScores.AsNoTracking()
            .OrderByDescending(s => s.Score)
            .ThenBy(s => s.CreatedAt)
            .Take(10)
            .Select(s => new { s.Id, s.PlayerName, s.Score, s.CreatedAt })
            .ToListAsync(cancellationToken);

        var idsActivos = jugadores.Select(j => j.Id).ToHashSet();
        // Si un jugador fue eliminado, no contamos sus partidas/logros en el detalle activo.
        var partidasActivas = partidas.Where(p => idsActivos.Contains(p.JugadorId)).ToArray();
        var logrosActivos = logros.Where(l => idsActivos.Contains(l.JugadorId)).ToArray();

        var detalle = jugadores.Select(j => new
        {
            id = j.Id,
            nombre = j.Nombre,
            alias = j.Alias,
            partidasJugadas = partidasActivas.Count(p => p.JugadorId == j.Id),
            puntosAcumulados = partidasActivas.Where(p => p.JugadorId == j.Id).Sum(p => p.Puntaje),
            logrosObtenidos = logrosActivos.Count(l => l.JugadorId == j.Id)
        })
        .OrderByDescending(j => j.puntosAcumulados)
        .ThenBy(j => j.nombre)
        .ToArray();

        return Ok(new
        {
            pais = "Argentina",
            autor = "Adalid Callizaya",
            proposito = "Rendimiento de jugadores, partidas, logros y mejores puntajes activos",
            tablas = new[]
            {
                "jugadores_argentina", "partidas_argentina",
                "logros_argentina", "leaderboard_argentina"
            },
            generadoEn = DateTime.UtcNow,
            totales = new
            {
                jugadores = jugadores.Count,
                partidas = partidasActivas.Length,
                logros = logrosActivos.Length,
                puntos = partidasActivas.Sum(p => p.Puntaje),
                mejorPuntaje = ranking.Count > 0 ? ranking[0].Score : 0
            },
            jugadores = detalle,
            mejoresPuntajes = ranking
        });
    }
}
