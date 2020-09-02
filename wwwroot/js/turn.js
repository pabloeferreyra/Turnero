function ConfirmAccess(uniqueId, isAccessedClicked) {
    var accessedSpan = 'accessedSpan_' + uniqueId;
    var confirmaccessedSpan = 'confirmaccessedSpan_' + uniqueId;

    if (isAccessedClicked) {
        $('#' + accessedSpan).hide();
        $('#' + confirmaccessedSpan).show();
    } else {
        $('#' + accessedSpan).show();
        $('#' + confirmaccessedSpan).hide();
    }
}

function accessed(urlAction, id) {
    var form = $('#__AjaxAntiForgeryForm');
    var token = $('input[name="__RequestVerificationToken"]', form).val();
    $.ajax({
        type: "POST",
        url: urlAction,
        data: {
            __RequestVerificationToken: token,
            id: id
        },
        success: function (result) {
            $("#TurnsPartial").html(result);
        },
        error: function (req, status, error) {
        }
    });
}

function Delete(urlAction, id) {
    var form = $('#__AjaxAntiForgeryForm');
    var token = $('input[name="__RequestVerificationToken"]', form).val();
    $.ajax({
        type: "POST",
        url: urlAction,
        data: {
            __RequestVerificationToken: token,
            id: id
        },
        success: function (result) {
            $("#TurnsPartial").html(result);
        },
        error: function (req, status, error) {
        }
    });
}

function ConfirmDelete(uniqueId, isAccessedClicked) {
    var accessedSpan = 'accessedSpan_' + uniqueId;
    var confirmaccessedSpan = 'confirmaccessedSpan_' + uniqueId;

    if (isAccessedClicked) {
        $('#' + accessedSpan).hide();
        $('#' + confirmaccessedSpan).show();
    } else {
        $('#' + accessedSpan).show();
        $('#' + confirmaccessedSpan).hide();
    }
}

function Create(urlAction) {
    var form = $('#__AjaxAntiForgeryForm');
    var token = $('input[name="__RequestVerificationToken"]', form).val();
    $.ajax({
        type: "POST",
        url: urlAction,
        data: {
            __RequestVerificationToken: token,
            Name: $("#clientName").val(),
            Dni: $("#dniCliente").val(),
            MedicId: $("#medicId :selected").val(),
            TimeId: $("#timeTurn :selected").val(),
            DateTurn: $("#dateTurn").val(),
            SocialWork: $("#socialWork").val(),
            Reason: $("#reason").val()
        },
        dataType: "json",
        contentType: "application/json"
    });
}

function SearchTurns(urlAction) {
    var date = $("#DateTurn").val();
    var medic = $("#Medics :selected").val();
    $.ajax({
        type: "POST",
        url: urlAction,
        data: {
            dateTurn: date,
            medicId: medic
        },
        success: function (result) {
            $("#TurnsPartial").html(result);
        },
        error: function (req, status, error) {
        }
    });
}
function SearchAllTurns(urlAction) {
    $.ajax({
        type: "POST",
        url: urlAction,
        data: {
            dateTurn: null,
            medicId: null
        },
        success: function (result) {
            $("#TurnsPartial").html(result);
        },
        error: function (req, status, error) {
        }
    });
}