(() => {
    "use strict";

    const SERVERS = [
        { id: "bolivia", name: "Bolivia", flag: "🇧🇴", endpoint: "/api/leaderboard/bolivia" },
        { id: "peru", name: "Perú", flag: "🇵🇪", endpoint: "/api/leaderboard/peru" },
        { id: "argentina", name: "Argentina", flag: "🇦🇷", endpoint: "/api/leaderboard/argentina" }
    ];

    const GAME_SECONDS = 30;
    const MAX_CIRCLES = 6;
    const CIRCLE_LIFETIME_MS = 1400;
    const SPAWN_DELAY_MS = 90;
    const COLORS = ["#6ee7b7", "#7dd3fc", "#f9a8d4", "#fde68a", "#c4b5fd", "#fb923c"];

    const ui = {
        serverSelect: document.getElementById("serverSelect"),
        serverName: document.getElementById("serverName"),
        timeValue: document.getElementById("timeValue"),
        scoreValue: document.getElementById("scoreValue"),
        timeBar: document.getElementById("timeBar"),
        gameArea: document.getElementById("gameArea"),
        startOverlay: document.getElementById("startOverlay"),
        endOverlay: document.getElementById("endOverlay"),
        finalScore: document.getElementById("finalScore"),
        playerName: document.getElementById("playerName"),
        saveButton: document.getElementById("saveButton"),
        saveMessage: document.getElementById("saveMessage"),
        playAgainButton: document.getElementById("playAgainButton"),
        startButton: document.getElementById("startButton"),
        startHint: document.getElementById("startHint"),
        statusText: document.getElementById("statusText"),
        leaderboardList: document.getElementById("leaderboardList"),
        leaderboardStatus: document.getElementById("leaderboardStatus"),
        leaderboardSubtitle: document.getElementById("leaderboardSubtitle"),
        refreshButton: document.getElementById("refreshButton")
    };

    const state = {
        running: false,
        score: 0,
        timeLeft: GAME_SECONDS,
        timerId: null,
        circles: new Set(),
        circleTimeouts: new Map(),
        spawnTimeouts: new Set(),
        selectedServer: SERVERS[0],
        saved: false
    };

    function currentServer() {
        return state.selectedServer;
    }

    function currentUser() {
        return window.appAuth?.state?.user ?? null;
    }

    function isAuthenticated() {
        return Boolean(currentUser());
    }

    function populateServers() {
        ui.serverSelect.replaceChildren(
            ...SERVERS.map((server) => {
                const option = document.createElement("option");
                option.value = server.id;
                option.textContent = `${server.flag} ${server.name}`;
                return option;
            })
        );
        ui.serverSelect.value = state.selectedServer.id;
    }

    function selectServer(id) {
        const server = SERVERS.find((item) => item.id === id);
        if (!server) return;

        state.selectedServer = server;
        ui.serverName.textContent = server.name;
        ui.leaderboardSubtitle.textContent = `Top 10 · ${server.name}`;
        loadLeaderboard();
    }

    function updateHud() {
        ui.timeValue.textContent = String(Math.max(0, state.timeLeft));
        ui.scoreValue.textContent = String(state.score);
        const ratio = Math.max(0, state.timeLeft) / GAME_SECONDS;
        ui.timeBar.style.width = `${ratio * 100}%`;
    }

    function clearCircle(circle) {
        const timeout = state.circleTimeouts.get(circle);
        if (timeout !== undefined) {
            clearTimeout(timeout);
        }
        state.circleTimeouts.delete(circle);
        state.circles.delete(circle);
        circle.remove();
    }

    function clearAllCircles() {
        for (const circle of Array.from(state.circles)) {
            clearCircle(circle);
        }
        for (const timeout of state.spawnTimeouts) {
            clearTimeout(timeout);
        }
        state.spawnTimeouts.clear();
    }

    function scheduleSpawn() {
        if (!state.running || state.circles.size >= MAX_CIRCLES) return;

        const timeout = setTimeout(() => {
            state.spawnTimeouts.delete(timeout);
            if (state.running && state.circles.size < MAX_CIRCLES) {
                spawnCircle();
            }
        }, SPAWN_DELAY_MS);

        state.spawnTimeouts.add(timeout);
    }

    function spawnCircle() {
        if (!state.running || state.circles.size >= MAX_CIRCLES) return;

        const area = ui.gameArea.getBoundingClientRect();
        const size = randomInt(42, 76);
        const maxX = Math.max(0, area.width - size);
        const maxY = Math.max(0, area.height - size);
        const x = Math.random() * maxX;
        const y = Math.random() * maxY;

        const circle = document.createElement("button");
        circle.type = "button";
        circle.className = "circle";
        circle.setAttribute("aria-label", "Círculo");
        circle.style.width = `${size}px`;
        circle.style.height = `${size}px`;
        circle.style.left = `${x}px`;
        circle.style.top = `${y}px`;
        circle.style.background = COLORS[randomInt(0, COLORS.length - 1)];
        circle.style.setProperty("--delay", `${Math.random() * 0.08}s`);

        circle.addEventListener(
            "click",
            () => {
                if (!state.running) return;
                state.score += 1;
                updateHud();
                clearCircle(circle);
                scheduleSpawn();
            },
            { once: true }
        );

        state.circles.add(circle);
        ui.gameArea.appendChild(circle);

        const timeout = setTimeout(() => {
            if (!state.running) return;
            clearCircle(circle);
            scheduleSpawn();
        }, CIRCLE_LIFETIME_MS);

        state.circleTimeouts.set(circle, timeout);
    }

    function startGame() {
        if (state.running) return;

        if (!isAuthenticated()) {
            showSaveMessage("Inicia sesión con Google o Discord para jugar y guardar.", true);
            ui.statusText.textContent = "La sesión es obligatoria para usar la API.";
            return;
        }

        clearAllCircles();
        state.running = true;
        state.score = 0;
        state.timeLeft = GAME_SECONDS;
        state.saved = false;

        ui.startOverlay.classList.add("hidden");
        ui.endOverlay.classList.add("hidden");
        ui.saveButton.disabled = !isAuthenticated();
        ui.saveButton.textContent = "Guardar puntuación";
        ui.saveMessage.textContent = "";
        ui.saveMessage.className = "message";
        ui.statusText.textContent = "¡A darle! Los círculos desaparecen rápido.";
        updateHud();

        for (let index = 0; index < MAX_CIRCLES; index += 1) {
            spawnCircle();
        }

        state.timerId = setInterval(() => {
            state.timeLeft -= 1;
            updateHud();
            if (state.timeLeft <= 0) {
                endGame();
            }
        }, 1000);
    }

    function endGame() {
        if (!state.running) return;

        state.running = false;
        if (state.timerId !== null) {
            clearInterval(state.timerId);
            state.timerId = null;
        }

        clearAllCircles();
        state.timeLeft = 0;
        updateHud();
        ui.finalScore.textContent = String(state.score);
        ui.endOverlay.classList.remove("hidden");
        ui.statusText.textContent = "Tiempo terminado. Guarda tu puntuación en el servidor elegido.";
        setTimeout(() => ui.playerName.focus(), 100);
    }

    async function saveScore() {
        if (state.saved) return;

        if (!isAuthenticated()) {
            showSaveMessage("Inicia sesión con Google o Discord antes de guardar.", true);
            return;
        }

        const playerName = ui.playerName.value.trim();
        if (!playerName) {
            showSaveMessage("Escribe tu nombre antes de guardar.", true);
            ui.playerName.focus();
            return;
        }
        if (playerName.length > 30) {
            showSaveMessage("El nombre no puede superar los 30 caracteres.", true);
            return;
        }

        const server = currentServer();
        ui.saveButton.disabled = true;
        ui.saveButton.textContent = "Guardando...";
        showSaveMessage("", false);

        try {
            const response = await fetch(server.endpoint, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                credentials: "same-origin",
                body: JSON.stringify({ playerName, score: state.score })
            });

            if (!response.ok) {
                if (response.status === 401) {
                    throw new Error("Tu sesión expiró. Vuelve a iniciar sesión.");
                }

                if (response.status === 404) {
                    throw new Error(
                        `El servidor ${server.name} aún no tiene tabla/controlador. Pide a tu compañero que lo integre.`
                    );
                }

                let detail = "";
                try {
                    const error = await response.json();
                    detail = error.message || error.title || "";
                } catch (_) {
                    detail = "";
                }
                throw new Error(detail || `No se pudo guardar (HTTP ${response.status}).`);
            }

            state.saved = true;
            localStorage.setItem("circle-rush-player", playerName);
            ui.saveButton.textContent = "Guardado";
            showSaveMessage(`Puntuación guardada en ${server.name}.`, false);
            await loadLeaderboard();
        } catch (error) {
            ui.saveButton.disabled = false;
            ui.saveButton.textContent = "Guardar puntuación";
            showSaveMessage(error.message, true);
        }
    }

    function showSaveMessage(text, isError) {
        ui.saveMessage.textContent = text;
        ui.saveMessage.className = isError ? "message error" : "message";
    }

    async function loadLeaderboard() {
        if (!isAuthenticated()) {
            ui.leaderboardList.replaceChildren();
            ui.leaderboardStatus.textContent = "Inicia sesión con Google o Discord para ver el leaderboard.";
            ui.leaderboardStatus.className = "message";
            return;
        }

        const server = currentServer();
        ui.leaderboardStatus.textContent = "Cargando leaderboard...";
        ui.leaderboardStatus.className = "message";
        ui.leaderboardList.replaceChildren();

        try {
            const response = await fetch(`${server.endpoint}?limit=10`, {
                headers: { Accept: "application/json" },
                credentials: "same-origin"
            });

            if (response.status === 401) {
                ui.leaderboardStatus.textContent = "Tu sesión expiró. Vuelve a iniciar sesión.";
                ui.leaderboardStatus.className = "message error";
                return;
            }

            if (response.status === 404) {
                ui.leaderboardStatus.textContent = `El servidor ${server.name} aún no está disponible.`;
                ui.leaderboardStatus.className = "message error";
                return;
            }

            if (!response.ok) {
                throw new Error(`HTTP ${response.status}`);
            }

            const scores = await response.json();
            if (!Array.isArray(scores) || scores.length === 0) {
                ui.leaderboardStatus.textContent = "Aún no hay puntuaciones. ¡Sé el primero!";
                return;
            }

            ui.leaderboardStatus.textContent = "";
            scores.forEach((entry, index) => {
                const item = document.createElement("li");

                const rank = document.createElement("span");
                rank.className = "rank";
                rank.textContent = String(index + 1);

                const info = document.createElement("span");
                info.className = "entry-info";
                const name = document.createElement("strong");
                name.textContent = entry.playerName;
                const date = document.createElement("span");
                date.className = "entry-date";
                date.textContent = formatDate(entry.createdAt);
                info.append(name, date);

                const points = document.createElement("span");
                points.className = "entry-score";
                points.textContent = `${entry.score} pts`;

                item.append(rank, info, points);
                ui.leaderboardList.appendChild(item);
            });
        } catch (error) {
            ui.leaderboardStatus.textContent = `No se pudo cargar el leaderboard: ${error.message}`;
            ui.leaderboardStatus.className = "message error";
        }
    }

    function formatDate(value) {
        const date = new Date(value);
        if (Number.isNaN(date.getTime())) return "";
        return date.toLocaleString("es-BO", { dateStyle: "short", timeStyle: "short" });
    }

    function randomInt(min, max) {
        return Math.floor(Math.random() * (max - min + 1)) + min;
    }

    function applyAuthState(authState) {
        const user = authState?.user ?? null;

        if (!user) {
            if (state.running) {
                state.running = false;
                if (state.timerId !== null) {
                    clearInterval(state.timerId);
                    state.timerId = null;
                }
                clearAllCircles();
                ui.startOverlay.classList.remove("hidden");
                ui.endOverlay.classList.add("hidden");
            }

            ui.startButton.disabled = true;
            ui.saveButton.disabled = true;
            ui.startHint.textContent =
                "Inicia sesión con Google o Discord para jugar. La API protege los endpoints.";
            ui.statusText.textContent = "Inicia sesión para jugar y guardar.";
            ui.leaderboardList.replaceChildren();
            ui.leaderboardStatus.textContent =
                "Inicia sesión con Google o Discord para ver el leaderboard.";
            ui.leaderboardStatus.className = "message";
            return;
        }

        ui.startButton.disabled = false;
        ui.saveButton.disabled = false;
        ui.startHint.textContent =
            `Sesión iniciada como ${user.name}. Haz clic en los círculos antes de que se acabe el tiempo.`;
        ui.statusText.textContent = "Sesión iniciada. Elige un servidor y empieza a jugar.";

        if (!ui.playerName.value.trim()) {
            ui.playerName.value = user.name || user.email || "";
        }

        loadLeaderboard();
    }

    ui.startButton.addEventListener("click", startGame);
    ui.playAgainButton.addEventListener("click", startGame);
    ui.saveButton.addEventListener("click", saveScore);
    ui.refreshButton.addEventListener("click", loadLeaderboard);
    ui.playerName.addEventListener("keydown", (event) => {
        if (event.key === "Enter") {
            event.preventDefault();
            saveScore();
        }
    });
    ui.serverSelect.addEventListener("change", () => selectServer(ui.serverSelect.value));

    populateServers();
    const savedName = localStorage.getItem("circle-rush-player");
    if (savedName) {
        ui.playerName.value = savedName;
    }
    selectServer(state.selectedServer.id);
    updateHud();

    if (window.appAuth) {
        window.appAuth.onChange(applyAuthState);
    } else {
        ui.startHint.textContent = "No se pudo cargar el módulo de autenticación.";
        ui.statusText.textContent = "Recarga la página para intentar de nuevo.";
    }
})();
