/// <reference path="https://cdnjs.cloudflare.com/ajax/libs/microsoft-signalr/7.0.5/signalr.min.js" />
/// <reference path="../js/turn.js" />
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/TurnsTableHub")
    .configureLogging(signalR.LogLevel.Information)
    .build();

function formatDate(date) {
    var day = date.getDate();
    var month = date.getMonth() + 1; // Los meses en JavaScript van de 0 a 11
    var year = date.getFullYear();

    // Asegurarse de que day y month tengan dos dígitos

    if (month < 10) {
        month = '0' + month;
    }

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
            icon: "/favicon.ico" // Ruta a una imagen para el ícono de la notificación
        };
        var currentDate = new Date();
        currentDate = formatDate(currentDate);
        if (date === currentDate) {
            new Notification(user + " Hay nuevos turnos", options);
            console.log("notificacion");
        }
    } else {
        Notification.requestPermission(function (permission) {
            if (permission === "granted") {
                var options = {
                    body: message,
                    icon: "/favicon.ico" // Ruta a una imagen para el ícono de la notificación
                };
                var currentDate = new Date();
                currentDate = formatDate(currentDate);
                if (date === currentDate) {
                    new Notification(user + " Hay nuevos turnos", options);
                    console.log("notificacion 2");
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
            icon: 'success',
            title: "Conexion Exitosa",
            showConfirmButton: false,
            timer: 100
        });
    })
    .catch(function (err) {
        return console.error(err.toString());
    });