$("#EditButton").on('click', function (event) {
    Edit($('#Medics').val().trim(), $('#Medics :selected').text());
    $("#Edit").modal('toggle');
});

function Edit(id, name) {
    $.ajax({
        type: "GET",
        url: "/Availability/Edit",
        data: {
            id: id,
            name: name
        },
        success: function (data) {
            $("#EditFormContent").html(data);
        }
    })
}