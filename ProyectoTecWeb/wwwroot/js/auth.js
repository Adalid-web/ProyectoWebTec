(() => {
    "use strict";

    const ui = {
        loading: document.getElementById("authLoading"),
        guest: document.getElementById("authGuest"),
        user: document.getElementById("authUser"),
        providerButtons: document.getElementById("providerButtons"),
        name: document.getElementById("authName"),
        provider: document.getElementById("authProvider"),
        logout: document.getElementById("logoutButton")
    };

    const state = {
        ready: false,
        user: null,
        providers: []
    };

    const listeners = new Set();

    function notify() {
        for (const listener of listeners) {
            listener(state);
        }
        window.dispatchEvent(new CustomEvent("auth:changed", { detail: state }));
    }

    function render() {
        if (!ui.loading || !ui.guest || !ui.user) return;

        ui.loading.classList.add("hidden");

        if (state.user) {
            ui.guest.classList.add("hidden");
            ui.user.classList.remove("hidden");
            ui.name.textContent = state.user.name || "Usuario";
            ui.provider.textContent = `${state.user.provider}${state.user.email ? ` · ${state.user.email}` : ""}`;
            return;
        }

        ui.user.classList.add("hidden");
        ui.guest.classList.remove("hidden");
        ui.providerButtons.replaceChildren();

        if (state.providers.length === 0) {
            const message = document.createElement("span");
            message.className = "auth-empty";
            message.textContent = "Configura Google o Discord en el .env";
            ui.providerButtons.appendChild(message);
            return;
        }

        for (const provider of state.providers) {
            const link = document.createElement("a");
            link.className = `btn btn-oauth btn-oauth-${provider.id}`;
            link.href = `/login/${provider.id}`;
            link.title = `Continuar con ${provider.name}`;

            const icon = document.createElement("span");
            icon.className = "oauth-icon";
            icon.setAttribute("aria-hidden", "true");
            icon.textContent = provider.icon || "●";

            const label = document.createElement("span");
            label.textContent = provider.name;

            link.append(icon, label);
            ui.providerButtons.appendChild(link);
        }
    }

    async function loadProviders() {
        try {
            const response = await fetch("/api/auth/providers", {
                headers: { Accept: "application/json" },
                credentials: "same-origin"
            });

            if (!response.ok) {
                state.providers = [];
                return;
            }

            const providers = await response.json();
            state.providers = Array.isArray(providers) ? providers : [];
        } catch (_) {
            state.providers = [];
        }
    }

    async function loadUser() {
        try {
            const response = await fetch("/api/auth/me", {
                headers: { Accept: "application/json" },
                credentials: "same-origin"
            });

            if (response.status === 401) {
                state.user = null;
                return;
            }

            if (!response.ok) {
                state.user = null;
                return;
            }

            state.user = await response.json();
        } catch (_) {
            state.user = null;
        }
    }

    async function refresh() {
        await Promise.all([loadProviders(), loadUser()]);
        state.ready = true;
        render();
        notify();
        return state;
    }

    async function logout() {
        try {
            await fetch("/logout", {
                method: "POST",
                credentials: "same-origin",
                redirect: "follow"
            });
        } finally {
            state.user = null;
            render();
            notify();
        }
    }

    ui.logout?.addEventListener("click", () => {
        ui.logout.disabled = true;
        logout().finally(() => {
            ui.logout.disabled = false;
        });
    });

    window.appAuth = {
        get state() {
            return state;
        },
        refresh,
        logout,
        onChange(listener) {
            listeners.add(listener);
            if (state.ready) {
                listener(state);
            }
            return () => listeners.delete(listener);
        }
    };

    refresh();
})();
