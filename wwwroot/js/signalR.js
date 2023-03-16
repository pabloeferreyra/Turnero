/// <reference path="../js/signalr/dist/browser/signalr.min.js" />
/// <reference path="../js/turn.js" />
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/TurnsTableHub")
    .configureLogging(signalR.LogLevel.Information)
    .build();

connection.on("UpdateTable", function (user, message) {
    console.log(message);
    reset();
});

connection.start()
    .then(function () {
        Swal.fire({
            position: 'top-end',
            icon: 'success',
            title: "Conexion Exitosa",
            showConfirmButton: false,
            timer: 600
        });
    })
    .catch(function (err) {
        return console.error(err.toString());
    });