(() => {
    "use strict";
    const boton = document.getElementById("verReporteArgentina");
    const estado = document.getElementById("reporteArgentinaEstado");
    const resultado = document.getElementById("reporteArgentinaResultado");
    if (!boton || !estado || !resultado) return;

    function nodo(tag, texto, clase) {
        const e = document.createElement(tag);
        if (clase) e.className = clase;
        if (texto !== undefined && texto !== null) e.textContent = String(texto);
        return e;
    }

    function pintar(data) {
        resultado.replaceChildren();
        const totales = data.totales || {};
        const indicadores = [
            ["Jugadores activos", totales.jugadores ?? 0],
            ["Partidas activas", totales.partidas ?? 0],
            ["Logros activos", totales.logros ?? 0],
            ["Puntos de partidas", totales.puntos ?? 0],
            ["Mejor puntuacion", totales.mejorPuntaje ?? 0]
        ];
        const cuadrícula = nodo("div", undefined, "reporte-indicadores");
        for (const [nombre, valor] of indicadores) {
            const tarjeta = nodo("div", undefined, "reporte-indicador");
            tarjeta.append(nodo("small", nombre), nodo("strong", valor));
            cuadrícula.append(tarjeta);
        }
        resultado.append(cuadrícula);

        function agregarTabla(titulo, encabezados, filas) {
            resultado.append(nodo("h3", titulo));
            if (!Array.isArray(filas) || filas.length === 0) {
                resultado.append(nodo("p", "No hay registros activos para este apartado.", "message"));
                return;
            }
            const contenedor = nodo("div", undefined, "reporte-tabla-contenedor");
            const tabla = nodo("table", undefined, "reporte-tabla");
            const thead = document.createElement("thead");
            const tr = document.createElement("tr");
            encabezados.forEach(x => tr.append(nodo("th", x)));
            thead.append(tr);
            const tbody = document.createElement("tbody");
            filas.forEach(fila => {
                const row = document.createElement("tr");
                fila.forEach(c => row.append(nodo("td", c)));
                tbody.append(row);
            });
            tabla.append(thead, tbody);
            contenedor.append(tabla);
            resultado.append(contenedor);
        }
        agregarTabla("Rendimiento por jugador", ["Jugador", "Alias", "Partidas", "Puntos", "Logros"],
            (data.jugadores || []).map(j => [j.nombre, j.alias, j.partidasJugadas, j.puntosAcumulados, j.logrosObtenidos]));
        agregarTabla("Top 10 de puntajes", ["Jugador", "Puntuacion"],
            (data.mejoresPuntajes || []).map(s => [s.playerName, s.score]));
    }

    boton.addEventListener("click", async () => {
        boton.disabled = true;
        estado.textContent = "Consultando reporte protegido...";
        estado.classList.remove("error");
        resultado.replaceChildren();
        try {
            const response = await fetch("/api/reportes/argentina/resumen", {
                method: "GET",
                credentials: "same-origin",
                headers: { "Accept": "application/json" },
                cache: "no-store"
            });
            if (response.status === 401) {
                estado.textContent = "HTTP 401: inicia sesion para consultar este reporte.";
                estado.classList.add("error");
                return;
            }
            if (response.status === 403) {
                estado.textContent = "HTTP 403: el reporte solo permite sesiones Twitch o Slack.";
                estado.classList.add("error");
                return;
            }
            if (!response.ok) throw new Error("HTTP " + response.status);
            const data = await response.json();
            pintar(data);
            estado.textContent = "HTTP 200: reporte autorizado de Argentina.";
        } catch (error) {
            estado.textContent = "No se pudo consultar el reporte: " + error.message;
            estado.classList.add("error");
        } finally {
            boton.disabled = false;
        }
    });
})();
