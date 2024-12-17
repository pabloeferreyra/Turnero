/// <reference path="https://cdnjs.cloudflare.com/ajax/libs/microsoft-signalr/8.0.7/signalr.min.js" />
/// <reference path="../js/turn.js" />
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/TurnsTableHub")
    .configureLogging(signalR.LogLevel.Information)
    .build();

function formatDate(date) {
    var day = date.getDate();
    var month = date.getMonth() + 1; // Los meses en JavaScript van de 0 a 11
    var year = date.getFullYear();


    return day + '/' + month + '/' + year;
}

connection.on("UpdateTableDirected", function (user, message, date) {
    if (!("Notification" in window)) {
        Swal.fire({
            position: 'top-end',
            icon: 'info',
            title: "Este navegador no soporta notificaciones web",
            showConfirmButton: false,
            timer: 500
        });
    } else if (Notification.permission === "granted") {
        var options = {
            body: message,
            icon: "/favicon.ico"
        };
        var currentDate = new Date();
        currentDate = formatDate(currentDate);
        if (date === currentDate) {
            new Notification(user + " Hay nuevos turnos", options);
        }
    } else {
        Notification.requestPermission(function (permission) {
            if (permission === "granted") {
                var options = {
                    body: message,
                    icon: "/favicon.ico"
                };
                var currentDate = new Date();
                currentDate = formatDate(currentDate);
                if (date === currentDate) {
                    new Notification(user + " Hay nuevos turnos", options);
                }
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
            text: "Conexion Exitosa",
            color: "#fff",
            background: "#28a745",
            showConfirmButton: false,
            timer: 200
        });
    })
    .catch(function (err) {
        return console.error(err.toString());
    });