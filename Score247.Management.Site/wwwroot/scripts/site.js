// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
$(document).ready(function () {
    var timezone = sessionStorage.getItem("Timezone");
    if (timezone === null) {
        $('#inputTimezone').val("00:00:00");
    } else {
        $('#inputTimezone').val(timezone);
    }
    $('#inputTimezone').val();
    var tlt = $('#TimelineTable').DataTable({
        "columnDefs": [{
            "searchable": false,
            "orderable": false,
            "targets": 0
        }],
        "order": [[1, 'decs']],
        "dom": '<"top"if>rt<"bottom"><"clear">',
        iDisplayLength: -1
    });
    tlt.on('order.dt search.dt', function () {
        tlt.column(0, { search: 'applied', order: 'applied' }).nodes().each(function (cell, i) {
            cell.innerHTML = i + 1;
        });
    }).draw();
    var m = $('#mathes-table').DataTable({
        "columnDefs": [{
            "searchable": false,
            "orderable": false,
            "targets": 0
        }],
        "order": [[1, 'asc']],
        "dom": '<"top"if>rt<"bottom"><"clear">',
        aLengthMenu: [
            [25, 50, 100, 200, -1],
            [25, 50, 100, 200, "All"]
        ],
        iDisplayLength: -1
    });

    m.on('order.dt search.dt', function () {
        m.column(0, { search: 'applied', order: 'applied' }).nodes().each(function (cell, i) {
            cell.innerHTML = i + 1;
        });
    }).draw();

    var lt = $('#leagues-table').DataTable({
        "columnDefs": [{
            "searchable": false,
            "orderable": false,
            "targets": 0
        }],
        "order": [[1, 'asc']],
        "dom": '<"top"if>rt<"bottom"><"clear">',
        aLengthMenu: [
            [25, 50, 100, 200, -1],
            [25, 50, 100, 200, "All"]
        ],
        iDisplayLength: -1
    });

    lt.on('order.dt search.dt', function () {
        lt.column(0, { search: 'applied', order: 'applied' }).nodes().each(function (cell, i) {
            cell.innerHTML = i + 1;
        });
    }).draw();

    var t = $('#teams-table').DataTable({
        "columnDefs": [{
            "searchable": false,
            "orderable": false,
            "targets": 0
        }],
        "order": [[1, 'asc']],
        "dom": '<"top"if>rt<"bottom"><"clear">',
        aLengthMenu: [
            [25, 50, 100, 200, -1],
            [25, 50, 100, 200, "All"]
        ],
        iDisplayLength: -1
    });

    t.on('order.dt search.dt', function () {
        t.column(0, { search: 'applied', order: 'applied' }).nodes().each(function (cell, i) {
            cell.innerHTML = i + 1;
        });
    }).draw();

    $('input[name="EventDate"]').daterangepicker({
        locale: { format: 'DD/MM/YYYY' },
        singleDatePicker: true,
        showDropdowns: true,
        minYear: 1901,
        maxYear: parseInt(moment().format('YYYY'), 10)
    }, function (start, end, label) {
        var years = moment().diff(start, 'years');
    });

    $('.timezone-hidden').val(sessionStorage.getItem("Timezone"));
});

function timezoneChange(selectTimezone) {
    sessionStorage.setItem("Timezone", selectTimezone.value);
    $('.timezone-hidden').val(sessionStorage.getItem("Timezone"));
    $('#timezoneSelection').submit();
}

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