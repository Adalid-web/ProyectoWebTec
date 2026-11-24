(() => {
    "use strict";

    const ui = {
        area: document.getElementById("authArea"),
        loading: document.getElementById("authLoading"),
        guest: document.getElementById("authGuest"),
        user: document.getElementById("authUser"),
        providerButtons: document.getElementById("providerButtons"),
        name: document.getElementById("authName"),
        provider: document.getElementById("authProvider"),
        logout: document.getElementById("logoutButton")
    };

    const state = { ready: false, user: null, providers: [] };
    const listeners = new Set();

    // El aviso se añade con JavaScript para conservar index.html y no mezclar
    // esta PR con los cambios de la PR de reporte de Argentina.
    const notice = document.createElement("span");
    notice.className = "auth-message hidden";
    notice.setAttribute("role", "status");
    notice.setAttribute("aria-live", "polite");
    ui.area?.appendChild(notice);

    function showNotice(message = "", isError = false) {
        notice.textContent = message;
        notice.classList.toggle("hidden", !message);
        notice.classList.toggle("auth-message-error", isError);
    }

    function notify() {
        for (const listener of listeners) listener(state);
        window.dispatchEvent(new CustomEvent("auth:changed", { detail: state }));
    }

    function render() {
        if (!ui.loading || !ui.guest || !ui.user) return;
        ui.loading.classList.add("hidden");

        if (state.user) {
            ui.guest.classList.add("hidden");
            ui.user.classList.remove("hidden");
            ui.name.textContent = state.user.name || "Usuario";
            ui.provider.textContent =
                `${state.user.provider || "Sesión"}${state.user.email ? ` · ${state.user.email}` : ""}`;
            return;
        }

        ui.user.classList.add("hidden");
        ui.guest.classList.remove("hidden");
        ui.providerButtons.replaceChildren();

        if (state.providers.length === 0) {
            const label = document.createElement("span");
            label.className = "auth-empty";
            label.textContent = "No se pudieron consultar los proveedores. Revisa la API.";
            ui.providerButtons.appendChild(label);
            return;
        }

        for (const provider of state.providers) {
            const configured = provider.configured !== false;
            const button = document.createElement(configured ? "a" : "button");
            button.className = `btn btn-oauth btn-oauth-${provider.id}`;

            if (configured) {
                button.href = `/login/${encodeURIComponent(provider.id)}`;
                button.title = `Continuar con ${provider.name}`;
            } else {
                button.type = "button";
                button.disabled = true;
                button.classList.add("btn-oauth-unconfigured");
                button.title = `${provider.name}: configura sus credenciales OAuth para habilitarlo`;
                button.setAttribute("aria-label", `${provider.name}: no configurado`);
            }

            const icon = document.createElement("span");
            icon.className = "oauth-icon";
            icon.setAttribute("aria-hidden", "true");
            icon.textContent = provider.icon || "●";

            const label = document.createElement("span");
            label.textContent = provider.name;

            button.append(icon, label);
            ui.providerButtons.appendChild(button);
        }
    }

    async function loadProviders() {
        const response = await fetch("/api/auth/providers", {
            headers: { Accept: "application/json" },
            credentials: "same-origin",
            cache: "no-store"
        });
        if (!response.ok) throw new Error(`Proveedores: HTTP ${response.status}`);
        const providers = await response.json();
        if (!Array.isArray(providers)) throw new Error("Respuesta inválida de proveedores.");
        state.providers = providers;
    }

    async function loadUser() {
        const response = await fetch("/api/auth/me", {
            headers: { Accept: "application/json" },
            credentials: "same-origin",
            cache: "no-store"
        });
        if (response.status === 401) {
            state.user = null;
            return;
        }
        if (!response.ok) throw new Error(`Sesión: HTTP ${response.status}`);
        state.user = await response.json();
    }

    async function refresh() {
        const results = await Promise.allSettled([loadProviders(), loadUser()]);
        for (const result of results) {
            if (result.status === "rejected") showNotice(result.reason?.message || "Error de conexión.", true);
        }
        state.ready = true;
        render();
        notify();
        return state;
    }

    async function logout() {
        if (ui.logout) ui.logout.disabled = true;
        showNotice();
        try {
            // En el servidor, /logout invalida la cookie. No basta con ocultar
            // el nombre en la interfaz: verificamos que /api/auth/me devuelva 401.
            const response = await fetch("/logout", {
                method: "POST",
                credentials: "same-origin",
                cache: "no-store",
                redirect: "follow"
            });
            if (!response.ok) throw new Error(`No se pudo cerrar sesión (HTTP ${response.status}).`);

            const confirmation = await fetch("/api/auth/me", {
                headers: { Accept: "application/json" },
                credentials: "same-origin",
                cache: "no-store"
            });
            if (confirmation.status !== 401) {
                throw new Error(`La API no confirmó el cierre de sesión (HTTP ${confirmation.status}).`);
            }

            state.user = null;
            state.ready = true;
            render();
            notify();
            showNotice("Sesión cerrada correctamente. Los recursos protegidos requieren volver a entrar.");
        } catch (error) {
            showNotice(error.message || "Error al cerrar sesión.", true);
            // Recupera el estado real de la sesión, sin fingir que se cerró.
            await refresh();
        } finally {
            if (ui.logout) ui.logout.disabled = false;
        }
    }

    ui.logout?.addEventListener("click", logout);

    window.appAuth = {
        get state() { return state; },
        refresh,
        logout,
        onChange(listener) {
            listeners.add(listener);
            if (state.ready) listener(state);
            return () => listeners.delete(listener);
        }
    };

    if (new URLSearchParams(window.location.search).get("auth_error") === "tiktok") {
        showNotice("No se completó el inicio de sesión con TikTok. Revisa la configuración de Sandbox antes de reintentar.", true);
        window.history.replaceState(null, "", window.location.pathname);
    }
    refresh();
})();

// PRUEBA LOCAL: Single Sign-Out de Circle Rush (NO cierra twitch.tv o slack.com).
(() => {
    "use strict";
    const logoutButton = document.getElementById("logoutButton");
    if (!logoutButton || document.getElementById("logoutAllButton")) return;

    const button = document.createElement("button");
    button.id = "logoutAllButton";
    button.type = "button";
    button.className = logoutButton.className;
    button.textContent = "Salir de todos";
    button.title = "Invalida todas tus sesiones de Circle Rush para este proveedor, incluso en otros navegadores";
    button.style.marginLeft = "6px";
    logoutButton.insertAdjacentElement("afterend", button);

    button.addEventListener("click", async () => {
        if (!window.confirm("¿Cerrar TODAS tus sesiones de Circle Rush con esta cuenta, incluso en otros equipos?")) return;
        button.disabled = true;
        try {
            const response = await fetch("/api/auth/logout/all", {
                method: "POST",
                credentials: "same-origin",
                headers: { Accept: "application/json" },
                cache: "no-store"
            });
            if (!response.ok) throw new Error(`Error de salida global: HTTP ${response.status}`);
            const confirmation = await fetch("/api/auth/me", {
                credentials: "same-origin", cache: "no-store"
            });
            if (confirmation.status !== 401) throw new Error("La API aún reconoce esta sesión.");
            await window.appAuth.refresh();
            window.alert("Se cerraron las sesiones de Circle Rush de esta cuenta. Las sesiones del proveedor externo no se cierran.");
        } catch (error) {
            window.alert(error.message || "No se pudo cerrar la sesión en todos los equipos.");
            await window.appAuth.refresh();
        } finally {
            button.disabled = false;
        }
    });

    // Al volver a la pestaña, refleja la invalidación hecha desde otro dispositivo.
    window.addEventListener("focus", () => { void window.appAuth.refresh(); });
})();
