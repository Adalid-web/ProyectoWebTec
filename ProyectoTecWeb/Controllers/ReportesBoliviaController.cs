using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoTecWeb.Data;

namespace ProyectoTecWeb.Controllers;

/// <summary>
/// Proyecto Final - reporte de Bolivia. Autor: Leonardo Julio.
/// Combina las cuatro tablas de Bolivia y respeta los filtros de soft delete.
/// </summary>
[ApiController]
[Authorize(Policy = "ReporteBolivia")]
[Route("api/reportes/bolivia")]
[Tags("Reportes - Bolivia")]
public sealed class ReportesBoliviaController : ControllerBase
{
    private readonly AppDbContext _db;

    public ReportesBoliviaController(AppDbContext db) => _db = db;

    [HttpGet("resumen")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Resumen(CancellationToken cancellationToken)
    {
        // Los filtros globales de EF Core excluyen los registros con soft delete.
        var jugadores = await _db.JugadoresBolivia.AsNoTracking()
            .Select(j => new { j.Id, j.Nombre, j.Alias })
            .ToListAsync(cancellationToken);

        var partidas = await _db.PartidasBolivia.AsNoTracking()
            .Select(p => new { p.JugadorId, p.Puntaje, p.DuracionSegundos })
            .ToListAsync(cancellationToken);

        var logros = await _db.LogrosBolivia.AsNoTracking()
            .Select(l => new { l.JugadorId, l.Nombre })
            .ToListAsync(cancellationToken);

        var ranking = await _db.BoliviaScores.AsNoTracking()
            .OrderByDescending(s => s.Score)
            .ThenBy(s => s.CreatedAt)
            .Take(10)
            .Select(s => new { s.Id, s.PlayerName, s.Score, s.CreatedAt })
            .ToListAsync(cancellationToken);

        var idsActivos = jugadores.Select(j => j.Id).ToHashSet();
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
            pais = "Bolivia",
            autor = "Leonardo Julio",
            proposito = "Rendimiento de jugadores, partidas, logros y mejores puntajes activos",
            tablas = new[]
            {
                "jugadores_bolivia", "partidas_bolivia",
                "logros_bolivia", "leaderboard_bolivia"
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
