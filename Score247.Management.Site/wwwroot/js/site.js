// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
$(document).ready(function () {
    $('#example').DataTable();
    $('#mathes-table').DataTable({
        aLengthMenu: [
            [25, 50, 100, 200, -1],
            [25, 50, 100, 200, "All"]
        ],
        iDisplayLength: -1
    });
});

function scoreEventTypeChange(selectObject) {
    console.log(selectObject.value);
    if (selectObject.value === 'normal_goal') {
        $('#scoreAssist').removeClass('d-none');
    } else {
        $('#scoreAssist').addClass('d-none');
    }
}

function scorePeriodTypeChange(selectObject) {
    console.log(selectObject.value);
    if (selectObject.value === 'extra_time') {
        $('#scoreExtratime').removeClass('d-none');
    } else {
        $('#scoreExtratime').addClass('d-none');
    }
}

function cardPeriodTypeChange(selectObject) {
    console.log(selectObject.value);
    if (selectObject.value === 'overtime') {
        $('#cardExtratime').removeClass('d-none');
    } else {
        $('#cardExtratime').addClass('d-none');
    }
}

function OpenModalPopUp(reservationDay) {
    //$('.reservationDay').text(reservationDay);
    var tmpData = JSON.parse(reservationDay);
    $(".reservationDay").text(JSON.stringify(tmpData, null, '\t'));
    $('#logRequest').modal();
}