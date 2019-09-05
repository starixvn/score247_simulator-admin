// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
$(document).ready(function () {
    $('input[name="CustomCreateMatchesDate"]').daterangepicker({
        singleDatePicker: true,
        locale: {
            format: 'DD/MM/YYYY'
        }
    });
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

    $('input[name="CustomEventDate"]').daterangepicker({
        locale: { format: 'MM/DD/YYYY' },
        singleDatePicker: true,
        showDropdowns: true,
        minYear: 2010,
        maxYear: parseInt(moment().format('YYYY'), 10)
    }, function (start, end, label) {
        var years = moment().diff(start, 'years');
    });

    $('.timezone-hidden').val(sessionStorage.getItem("Timezone"));

    $('#btnCreateAllSiteMatches').click(function (event) {
        $("#CreateMatchesSpinner").removeClass("d-none");
        event.preventDefault();
        console.log("Begin Send Post");
        var leagueLimit = $('#LeagueInputLimit').val();
        let createDeleteMatchesModels = [
            {
                EventDate: $('#3rePostEventDate').val(),
                Count: $('#3rePostCount').val(),
                LeagueLimit: leagueLimit
            },
            {
                EventDate: $('#2ndPostEventDate').val(),
                Count: $('#2ndPostCount').val(),
                LeagueLimit: leagueLimit
            },
            {
                EventDate: $('#1stPostEventDate').val(),
                Count: $('#1stPostCount').val(),
                LeagueLimit: leagueLimit
            },
            {
                EventDate: $('#todayEventDate').val(),
                Count: $('#todayCount').val(),
                LeagueLimit: leagueLimit
            },
            {
                EventDate: $('#1stPreEventDate').val(),
                Count: $('#1stPreCount').val(),
                LeagueLimit: leagueLimit
            },
            {
                EventDate: $('#2ndPreEventDate').val(),
                Count: $('#2ndPreCount').val(),
                LeagueLimit: leagueLimit
            }
        ];
        console.log("Post with Data", JSON.stringify(createDeleteMatchesModels));
        $.ajax({
            type: "POST",
            url: urlPrefix() + "/Match/CreateAllSiteMatches",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            data: JSON.stringify(createDeleteMatchesModels),
            success: function (d) {
                $("#CreateMatchesSpinner").addClass("d-none");
            },
            error: function (xhr, textStatus, errorThrown) {
                // TODO: Show error
            }
        });
    });

    $('#btnCreateMatch3rdPost').click(function (event) {
        $("#CreateMatchesSpinner").removeClass("d-none");
        event.preventDefault();
        SendCreateMatchesByDate("3rdPost");
    });
    $('#btnCreateMatch2ndPost').click(function (event) {
        $("#CreateMatchesSpinner").removeClass("d-none");
        event.preventDefault();
        SendCreateMatchesByDate("2ndPost");
    });
    $('#btnCreateMatch1stPost').click(function (event) {
        $("#CreateMatchesSpinner").removeClass("d-none");
        event.preventDefault();
        SendCreateMatchesByDate("1stPost");
    });
    $('#btnCreateMatchToday').click(function (event) {
        $("#CreateMatchesSpinner").removeClass("d-none");
        event.preventDefault();
        SendCreateMatchesByDate("Today");
    });
    $('#btnCreateMatch1stPre').click(function (event) {
        $("#CreateMatchesSpinner").removeClass("d-none");
        event.preventDefault();
        SendCreateMatchesByDate("1stPre");
    });
    $('#btnCreateMatch2ndPre').click(function (event) {
        $("#CreateMatchesSpinner").removeClass("d-none");
        event.preventDefault();
        SendCreateMatchesByDate("2ndPre");
    });
    $('#btnCreateMatchCustom').click(function (event) {
        $("#CreateMatchesSpinner").removeClass("d-none");
        event.preventDefault();
        SendCreateMatchesByDate("Custom");
    });

    $('#btnDeleteMatch3rdPost').click(function (event) {
        $("#CreateMatchesSpinner").removeClass("d-none");
        event.preventDefault();
        SendDeleteMatchesByDate("3rdPost");
    });
    $('#btnDeleteMatch2ndPost').click(function (event) {
        $("#CreateMatchesSpinner").removeClass("d-none");
        event.preventDefault();
        SendDeleteMatchesByDate("2ndPost");
    });
    $('#btnDeleteMatch1stPost').click(function (event) {
        $("#CreateMatchesSpinner").removeClass("d-none");
        event.preventDefault();
        SendDeleteMatchesByDate("1stPost");
    });
    $('#btnDeleteMatchToday').click(function (event) {
        $("#CreateMatchesSpinner").removeClass("d-none");
        event.preventDefault();
        SendDeleteMatchesByDate("Today");
    });
    $('#btnDeleteMatch1stPre').click(function (event) {
        $("#CreateMatchesSpinner").removeClass("d-none");
        event.preventDefault();
        SendDeleteMatchesByDate("1stPre");
    });
    $('#btnDeleteMatch2ndPre').click(function (event) {
        $("#CreateMatchesSpinner").removeClass("d-none");
        event.preventDefault();
        SendDeleteMatchesByDate("2ndPre");
    });
    $('#btnDeleteMatchCustom').click(function (event) {
        $("#CreateMatchesSpinner").removeClass("d-none");
        event.preventDefault();
        SendDeleteMatchesByDate("Custom");
    });

    $('#btnCreateLiveMatches').click(function (event) {
        $("#CreateMatchesSpinner").removeClass("d-none");
        event.preventDefault();
        SendCreateLiveMatches();
    });
    $('#btnDeleteLiveMatches').click(function (event) {
        $("#CreateMatchesSpinner").removeClass("d-none");
        event.preventDefault();
        SendDeleteLiveMatches();
    });

    $('#btnConfirmDeleteAllMatches').click(function () {
        SendDeleteAllMatches();
    });
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

function UpdateLimitMatchInput(limitCount) {
    console.log(limitCount);
    $('.LeagueLimit').val(limitCount);
}

function SendCreateMatchesByDate(prefixType) {
    var eventDate = $('#' + prefixType + 'EventDate').val();
    var limitMatches = $('#' + prefixType + 'Count').val();
    var leagueLimit = $('#LeagueInputLimit').val();
    var createDeleteMatchesRequest = {
        EventDate: eventDate,
        Count: limitMatches,
        LeagueLimit: leagueLimit
    };

    $.ajax({
        type: "POST",
        url: urlPrefix() + "/Match/CreateMatchesByDate",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        data: JSON.stringify(createDeleteMatchesRequest),
        success: function () {
            $("#CreateMatchesSpinner").addClass("d-none");
        },
        error: function () {
        }
    });
}

function SendDeleteMatchesByDate(prefixType) {
    var eventDate = $('#' + prefixType + 'EventDate').val();
    var limitMatches = $('#' + prefixType + 'Count').val();
    var leagueLimit = $('#LeagueInputLimit').val();
    var createDeleteMatchesRequest = {
        EventDate: eventDate,
        Count: limitMatches,
        LeagueLimit: leagueLimit
    };

    $.ajax({
        type: "POST",
        url: urlPrefix() + "/Match/DeleteMatchesByDate",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        data: JSON.stringify(createDeleteMatchesRequest),
        success: function () {
            $("#CreateMatchesSpinner").addClass("d-none");
        },
        error: function () {
        }
    });
}

function SendCreateLiveMatches() {
    var limitMatches = $('#LiveCount').val();
    var leagueLimit = $('#LeagueInputLimit').val();
    var createDeleteMatchesRequest = {
        Count: limitMatches,
        LeagueLimit: leagueLimit
    };

    $.ajax({
        type: "POST",
        url: urlPrefix() + "/Match/CreateLiveMatches",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        data: JSON.stringify(createDeleteMatchesRequest),
        success: function () {
            $("#CreateMatchesSpinner").addClass("d-none");
        },
        error: function () {
        }
    });
}

function SendDeleteLiveMatches() {
    var limitMatches = $('#LiveCount').val();
    var leagueLimit = $('#LeagueInputLimit').val();
    var createDeleteMatchesRequest = {
        Count: limitMatches,
        LeagueLimit: leagueLimit
    };

    $.ajax({
        type: "POST",
        url: urlPrefix() + "/Match/DeleteLiveMatches",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        data: JSON.stringify(createDeleteMatchesRequest),
        success: function () {
            $("#CreateMatchesSpinner").addClass("d-none");
        },
        error: function () {
        }
    });
}

function SendDeleteAllMatches() {
    $.ajax({
        type: "POST",
        url: urlPrefix() + "/Match/DeleteAllMatches",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function () {
        },
        error: function () {
        }
    });
}

function ShortcutSearchByDate(searchDate) {
    var selectedLeagueId = $('#leagueFilterId').val();
    var searchRequest = {
        SearchDate: searchDate,
        SelectedLeagueId: selectedLeagueId
    };

    $.ajax({
        type: "POST",
        url: urlPrefix() + "/Match/ShortcutSearchByDate",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        data: JSON.stringify(searchRequest),
        success: function () {
        },
        error: function () {
        }
    });
}

function getRootUrl() {
    return window.location.origin
        ? window.location.origin + '/'
        : window.location.protocol + '/' + window.location.host + '/';
}

function urlPrefix() {
    return getRootUrl() + $('#urlPrefix').val();
}