#!/usr/bin/env bash
set -uo pipefail

fallos=0
imagen_referencia=""
volumen_referencia=""

for numero in 1 2 3 4; do
    puerto=$((5000 + numero))
    contenedor="proyectotecweb-api-$numero"

    echo "===================================="
    echo "API $numero - Puerto $puerto"
    echo "===================================="

    for prueba in \
        "health:200" \
        "health/db:200" \
        "api/reportes/argentina/resumen:401"
    do
        ruta="${prueba%:*}"
        esperado="${prueba##*:}"

        obtenido=$(curl -s -o /dev/null \
            -w '%{http_code}' \
            --max-time 5 \
            "http://127.0.0.1:$puerto/$ruta" || true)

        echo "/$ruta -> HTTP $obtenido (esperado $esperado)"

        if [[ "$obtenido" != "$esperado" ]]; then
            fallos=$((fallos + 1))
        fi
    done

    imagen=$(docker inspect \
        --format '{{.Config.Image}}' \
        "$contenedor" 2>/dev/null || true)

    volumen=$(docker inspect \
        --format '{{range .Mounts}}{{if eq .Destination "/auth-keys"}}{{.Name}}{{end}}{{end}}' \
        "$contenedor" 2>/dev/null || true)

    echo "Imagen: $imagen"
    echo "Volumen de claves: $volumen"

    if [[ -z "$imagen" || -z "$volumen" ]]; then
        fallos=$((fallos + 1))
    fi

    if [[ "$numero" -eq 1 ]]; then
        imagen_referencia="$imagen"
        volumen_referencia="$volumen"
    else
        if [[ "$imagen" != "$imagen_referencia" ||
              "$volumen" != "$volumen_referencia" ]]; then
            fallos=$((fallos + 1))
        fi
    fi

    echo
done

if [[ "$fallos" -eq 0 ]]; then
    echo "RESULTADO: TODAS LAS VERIFICACIONES CORRECTAS"
else
    echo "RESULTADO: $fallos VERIFICACIONES FALLIDAS"
    exit 1
fi
