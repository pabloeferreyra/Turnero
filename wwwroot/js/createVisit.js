var currentDate = "";

$(document).ready(function () {

    var tdate = new Date();
    var dd = tdate.getDate(); 
    var MM = tdate.getMonth() + 1; 
    var yyyy = tdate.getFullYear(); 
    if (dd < 10) {
        dd = '0' + dd;
    }

    if (MM < 10) {
        MM = '0' + MM;
    }

    var isoDate = yyyy + "-" + MM + "-" + dd;
    currentDate = isoDate;
    var $dateInput = $('#VisitDate');
    if ($dateInput.length) {
        $dateInput.val(isoDate);
    }
});

function validateDate($input, $btn) {
    var val = $input.val();
    if (!val) { $btn.prop('disabled', false); return true; }
    var d = new Date(val + 'T00:00:00');
    var today = new Date(); today.setHours(0, 0, 0, 0);
    if (d > today) {
        Swal.fire({ position: 'top-end', icon: 'info', title: 'No puede seleccionar fechas posteriores a hoy.', showConfirmButton: false, timer: 1200 });
        $("#btnCreateVisit").prop('disabled', true);
        return false;
    }

    $("#btnCreateVisit").prop('disabled', false);
    return true;
}

$("#btnCreateVisit").on('click', function (event) {
    event.preventDefault();
    console.log('create click');
    /*Create();*/
});

function Create() {
    var token = $('input[name="__RequestVerificationToken"]', '#__AjaxAntiForgeryForm').val();
    var obj = $("#CreateVisitForm").serializeArray().reduce(function(a,b){ a[b.name]=b.value; return a; }, {});
    $.ajax({
        type: "POST",
        url: "/Visits/Create",
        contentType: 'application/json; charset=UTF-8',
        data: JSON.stringify(obj),
        headers: { 'RequestVerificationToken': token },
        success: function () {
            $("#CreateVisit").modal('toggle');
            reset();
            Swal.fire({
                position: 'top-end',
                icon: 'success',
                title: 'Visita creada correctamente.',
                showConfirmButton: false,
                timer: 600
            });
        },
    });
}

(function () {
    function todayString() {
        var d = new Date();
        var yyyy = d.getFullYear();
        var mm = (d.getMonth() + 1).toString().padStart(2, '0');
        var dd = d.getDate().toString().padStart(2, '0');
        return yyyy + '-' + mm + '-' + dd;
    }

    var $input = $('#VisitDate');
    var $btn = $('#btnCreateVisit');

    if ($input.length) {
        $input.attr('max', todayString());
    }

    $(document).on('change input', '#VisitDate', function () { validateDate($input, $btn); });

    $(function () { if ($input.length) validateDate($input, $btn); });
})();
