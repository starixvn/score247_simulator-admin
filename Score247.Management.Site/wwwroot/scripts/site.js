// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
$(document).ready(function () {
    $('#inputTimezone').val(localStorage.getItem("currentTimezone"));
    $('#example').DataTable();
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
});

function timezoneChange(selectTimezone) {
    localStorage.setItem("currentTimezone", selectTimezone.value);
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