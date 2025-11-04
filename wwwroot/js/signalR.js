/// <reference path="https://cdnjs.cloudflare.com/ajax/libs/microsoft-signalr/9.0.6/signalr.min.js" />
/// <reference path="../js/turn.js" />

// connection
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/TurnsTableHub")
    .configureLogging(signalR.LogLevel.Information)
    .build();

// helpers
const todayISO_DDMMYYYY = () => {
    const now = new Date();
    return now.toLocaleDateString("es-AR"); // dd/mm/yyyy
};

// unified notification
function notifyIfForToday(user, message, dateStr) {
    const today = todayISO_DDMMYYYY();
    if (dateStr !== today) return;

    if (!("Notification" in window)) {
        AppUtils.showToast("info", "Este navegador no soporta notificaciones web");
        return;
    }

    const doNotify = () =>
        new Notification(`${user} Hay nuevos turnos`, {
            body: message,
            icon: "/favicon.ico"
        });

    if (Notification.permission === "granted") {
        doNotify();
    } else if (Notification.permission !== "denied") {
        Notification.requestPermission().then(p => p === "granted" && doNotify());
    }
}

// server -> this client only
connection.on("UpdateTableDirected", (user, message, date) => {
    notifyIfForToday(user, message, date);
    reset?.();
});

// server -> everyone
connection.on("UpdateTable", () => {
    reset?.();
});

// connect
connection.start()
    .then(() => AppUtils.showToast("success", "SignalR conectado", 300))
    .catch(err => console.error(err));
