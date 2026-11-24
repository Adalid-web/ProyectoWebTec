(() => {
    "use strict";

    const reportes = [
        {
            boton: document.getElementById("verReporteBolivia"),
            estado: document.getElementById("reporteBoliviaEstado"),
            resultado: document.getElementById("reporteBoliviaResultado"),
            endpoint: "/api/reportes/bolivia/resumen",
            pais: "Bolivia",
            permiso: "Google o Discord"
        },
        {
            boton: document.getElementById("verReportePeru"),
            estado: document.getElementById("reportePeruEstado"),
            resultado: document.getElementById("reportePeruResultado"),
            endpoint: "/api/reportes/peru/resumen",
            pais: "Perú",
            permiso: "Dropbox o Spotify"
        }
    ];

    function nodo(tag, texto, clase) {
        const elemento = document.createElement(tag);
        if (clase) elemento.className = clase;
        if (texto !== undefined && texto !== null) elemento.textContent = String(texto);
        return elemento;
    }

    function agregarTabla(contenedor, titulo, encabezados, filas) {
        contenedor.append(nodo("h3", titulo));
        if (!Array.isArray(filas) || filas.length === 0) {
            contenedor.append(nodo("p", "No hay registros para este apartado.", "message"));
            return;
        }

        const tablaContenedor = nodo("div", undefined, "reporte-tabla-contenedor");
        const tabla = nodo("table", undefined, "reporte-tabla");
        const thead = document.createElement("thead");
        const filaEncabezados = document.createElement("tr");
        encabezados.forEach((encabezado) => filaEncabezados.append(nodo("th", encabezado)));
        thead.append(filaEncabezados);

        const tbody = document.createElement("tbody");
        filas.forEach((fila) => {
            const tr = document.createElement("tr");
            fila.forEach((celda) => tr.append(nodo("td", celda)));
            tbody.append(tr);
        });

        tabla.append(thead, tbody);
        tablaContenedor.append(tabla);
        contenedor.append(tablaContenedor);
    }

    function pintar(contenedor, data) {
        contenedor.replaceChildren();
        const totales = data.totales || {};
        const indicadores = [
            ["Jugadores", totales.jugadores ?? 0],
            ["Partidas", totales.partidas ?? 0],
            ["Logros", totales.logros ?? 0],
            ["Puntos de partidas", totales.puntos ?? 0],
            ["Mejor puntaje", totales.mejorPuntaje ?? 0]
        ];

        const cuadricula = nodo("div", undefined, "reporte-indicadores");
        for (const [nombre, valor] of indicadores) {
            const tarjeta = nodo("div", undefined, "reporte-indicador");
            tarjeta.append(nodo("small", nombre), nodo("strong", valor));
            cuadricula.append(tarjeta);
        }
        contenedor.append(cuadricula);

        agregarTabla(
            contenedor,
            "Rendimiento por jugador",
            ["Jugador", "Alias", "Partidas", "Puntos", "Logros"],
            (data.jugadores || []).map((j) => [
                j.nombre,
                j.alias,
                j.partidasJugadas,
                j.puntosAcumulados,
                j.logrosObtenidos
            ])
        );

        agregarTabla(
            contenedor,
            "Top 10 de puntajes",
            ["Jugador", "Puntuación"],
            (data.mejoresPuntajes || []).map((s) => [s.playerName, s.score])
        );
    }

    for (const reporte of reportes) {
        if (!reporte.boton || !reporte.estado || !reporte.resultado) continue;

        reporte.boton.addEventListener("click", async () => {
            reporte.boton.disabled = true;
            reporte.estado.textContent = `Consultando reporte de ${reporte.pais}...`;
            reporte.estado.classList.remove("error");
            reporte.resultado.replaceChildren();

            try {
                const response = await fetch(reporte.endpoint, {
                    method: "GET",
                    credentials: "same-origin",
                    headers: { Accept: "application/json" },
                    cache: "no-store"
                });

                if (response.status === 401) {
                    reporte.estado.textContent = "HTTP 401: inicia sesión para consultar este reporte.";
                    reporte.estado.classList.add("error");
                    return;
                }

                if (response.status === 403) {
                    reporte.estado.textContent =
                        `HTTP 403: este reporte solo permite sesiones de ${reporte.permiso}.`;
                    reporte.estado.classList.add("error");
                    return;
                }

                if (!response.ok) throw new Error(`HTTP ${response.status}`);

                const data = await response.json();
                pintar(reporte.resultado, data);
                reporte.estado.textContent = `HTTP 200: reporte autorizado de ${reporte.pais}.`;
            } catch (error) {
                reporte.estado.textContent = `No se pudo consultar el reporte: ${error.message}`;
                reporte.estado.classList.add("error");
            } finally {
                reporte.boton.disabled = false;
            }
        });
    }
})();
