/// <reference path="https://cdnjs.cloudflare.com/ajax/libs/microsoft-signalr/7.0.5/signalr.min.js" />
/// <reference path="../js/turn.js" />
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/TurnsTableHub")
    .configureLogging(signalR.LogLevel.Information)
    .build();

connection.on("UpdateTableDirected", function (user, message) {
    if (!("Notification" in window)) {
        alert("Este navegador no soporta notificaciones web");
    } else if (Notification.permission === "granted") {
        var options = {
            body: message,
            icon: "/favicon.ico" // Ruta a una imagen para el ícono de la notificación
        };
        var notification = new Notification(options, user + " Hay nuevos turnos");
    } else if (Notification.permission !== 'denied') {
        Notification.requestPermission(function (permission) {
            if (permission === "granted") {
                var options = {
                    body: message,
                    icon: "/favicon.ico" // Ruta a una imagen para el ícono de la notificación
                };
                var notification = new Notification(options, user + " Hay nuevos turnos");
            }
        });
    }
    reset();
})

connection.on("UpdateTable", function (message) {
    reset();
});

connection.start()
    .then(function () {
        Swal.fire({
            position: 'top-end',
            icon: 'success',
            title: "Conexion Exitosa",
            showConfirmButton: false,
            timer: 100
        });
    })
    .catch(function (err) {
        return console.error(err.toString());
    });