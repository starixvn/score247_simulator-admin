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
    $('#inputTimezone').val();
    $('#loadingDiv').hide();
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
            "searchable": true,
            "orderable": true,
            "targets": 0
        }],
        "dom": '<"top"if>rt<"bottom"><"clear">',
        iDisplayLength: -1
    });

    m.on('order.dt search.dt', function () {
        m.column(0, { search: 'applied', order: 'applied' }).nodes().each(function (cell, i) {
            cell.innerHTML = i + 1;
        });
    }).draw();

    $('#mathes-table tbody').on('click', 'tr.group', function () {
        var currentOrder = table.order()[0];
        if (currentOrder[0] === groupColumn && currentOrder[1] === 'asc') {
            table.order([groupColumn, 'desc']).draw();
        }
        else {
            table.order([groupColumn, 'asc']).draw();
        }
    });

    var lt = $('#leagues-table').DataTable({
        "columnDefs": [{
            "searchable": true,
            "orderable": true,
            "targets": 0
        }],
        "order": [[3, 'asc']],
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
            "searchable": true,
            "orderable": true,
            "targets": 0
        }],
        "order": [[1, 'asc']],
        aLengthMenu: [
            [25, 50, 100, 200, -1],
            [25, 50, 100, 200, "All"]
        ],
        iDisplayLength: 100
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
        minYear: 1990,
        maxYear: parseInt(moment().format('YYYY'), 50)
    }, function (start, end, label) {
        var years = moment().diff(start, 'years');
    });
    $('.customDatepicker').daterangepicker({
        locale: { format: 'MM/DD/YYYY' },
        singleDatePicker: true,
        showDropdowns: true,
        minYear: 1990,
        maxYear: parseInt(moment().format('YYYY'), 50)
    }, function (start, end, label) {
        var years = moment().diff(start, 'years');
    });

    $('.customDateTimepicker').daterangepicker({
        locale: { format: 'MM/DD/YYYY HH:mm:ss' },
        singleDatePicker: true,
        showDropdowns: true,
        minYear: 1990,
        startDate: moment().startOf('hour'),
        timePicker: true,
        drops: 'up',
        maxYear: parseInt(moment().format('YYYY'), 50)
    });

    $('.timezone-hidden').val(sessionStorage.getItem("Timezone"));

    $('#btnCreateAllSiteMatches').click(function (event) {
        $("#CreateMatchesSpinner").removeClass("d-none");
        event.preventDefault();
        console.log("Begin Send Post");
        var leagueLimit = $('#LeagueInputLimit').val();
        let createDeleteMatchesModels = [
            {
                EventDate: $('#3rdPostEventDate').val(),
                Count: $('#3rdPostCount').val(),
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
                EventDate: $('#TodayEventDate').val(),
                Count: $('#TodayCount').val(),
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

function scoreTimeEventTypeChange(selectObject) {
    console.log(selectObject.value);
    if (selectObject.value === 'match_end_pen' || selectObject.value === 'match_end_normal' || selectObject.value === 'match_end_extra') {
        $('#aggregate_input').removeClass('d-none');
    } else {
        $('#aggregate_input').addClass('d-none');
    }
}

function scorePeriodTypeChange(selectObject) {
    if (selectObject.value === 'extra_time') {
        $('#scoreExtratime').removeClass('d-none');
    } else {
        $('#scoreExtratime').addClass('d-none');
    }
}

function substitudePeriodTypeChange(selectObject) {
    console.log(selectObject.value);
    if (selectObject.value === 'extra_time') {
        $('#substitudeExtratime').removeClass('d-none');
    } else {
        $('#substitudeExtratime').addClass('d-none');
    }
}

function minorPeriodTypeChange(selectObject) {
    console.log(selectObject.value);
    if (selectObject.value === 'extra_time') {
        $('#minorExtratime').removeClass('d-none');
    } else {
        $('#minorExtratime').addClass('d-none');
    }
}

function cardPeriodTypeChange(selectObject) {
    console.log(selectObject.value);
    if (selectObject.value === 'extra_time') {
        $('#cardInjurytime').removeClass('d-none');
    } else {
        $('#cardInjurytime').addClass('d-none');
    }
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
    var preMatchCount = $('#LivePreCount').val();
    var liveMatchCount = $('#LiveCount').val();
    var postMatchCount = $('#LivePostCount').val();
    var leagueLimit = $('#LiveLeagueLimit').val();

    var createLiveMatchModel = {
        LiveCount: liveMatchCount,
        PreCount: preMatchCount,
        PostCount: postMatchCount,
        LeagueLimit: leagueLimit
    };

    $.ajax({
        type: "POST",
        url: urlPrefix() + "/Match/CreateLiveMatches",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        data: JSON.stringify(createLiveMatchModel),
        success: function () {
            HideLoadingSpinner('#listMatches');
        },
        error: function () {
            HideLoadingSpinner('#listMatches');
        }
    });
}

function SendCloseAllLiveMatches() {
    console.log("Begin close all matches");
    $.ajax({
        type: "POST",
        url: urlPrefix() + "/Match/CloseAllLiveMatches",
        contentType: "application/json; charset=utf-8",
        success: function () {
            HideLoadingSpinner('#listMatches');
        },
        error: function () {
            HideLoadingSpinner('#listMatches');
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
    console.log(searchDate);
    var selectedLeagueId = $('#leagueFilterId').val();
    var searchRequest = {
        SearchDate: moment(searchDate, "MM/DD/YYYY"),
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

function getBookMakerOddsMovement(matchId, bookmakerId) {
    $("#oddsMovement").html("");
    var betType = $("#betType").val();
    var betName = "1X2";

    if (betType === '1') {
        betName = "1X2";
    }
    if (betType === '3') {
        betName = "AsianHDP";
    }
    if (betType === '2') {
        betName = "OverUnder";
    }

    var oddsMovementRequestModel = {
        MatchId: matchId,
        BetType: betType,
        BookmakerId: bookmakerId,
        BetName: betName
    };
    $.ajax({
        async: true,
        url: urlPrefix() + '/Odds/GetOddsMovement',
        data: JSON.stringify(oddsMovementRequestModel),
        type: 'post',
        dataType: 'json',
        contentType: 'application/json; charset=utf-8',
        success: function (oddsMovementPartial) {
            $("#oddsMovement").html(oddsMovementPartial);
        },
        error: function (ex) {
            console.log(ex);
            $("#oddsMovement").html(ex.responseText);
        }
    });
}

function sendOddsChange(matchId) {
    console.log("Begin Send");
    event.preventDefault();
    var betType = $("#betType").val();
    var $inputs = $('#createOddsForm :input');

    var OddsChangeRequestModel = {
    };
    OddsChangeRequestModel["MatchId"] = matchId;
    OddsChangeRequestModel["BetType"] = betType;
    $inputs.each(function () {
        OddsChangeRequestModel[this.name] = $(this).val();
    });
    console.log(OddsChangeRequestModel);
    $.ajax({
        async: false,
        url: urlPrefix() + '/Odds/SendOddsMovement',
        data: JSON.stringify(OddsChangeRequestModel),
        type: 'post',
        dataType: 'json',
        contentType: 'application/json; charset=utf-8',
        success: function (result) {
            console.log(result);
        },
        error: function (ex) {
            console.log(ex);
        }
    });
}

function onBetTypeChange(matchId, betType) {
    $("#oddsComparison").html("");
    $("#oddsMovement").html("");
    var betName = "1X2";
    $(".oddsInput").addClass("d-none");
    if (betType.value === '1') {
        $(".1x2Odds").removeClass("d-none");
        betName = "1X2";
    }
    if (betType.value === '3') {
        $(".HDPOdds").removeClass("d-none");
        betName = "AsianHDP";
    }
    if (betType.value === '2') {
        $(".OUOdds").removeClass("d-none");
        betName = "OverUnder";
    }

    var oddsComparisonRequest = {
        MatchId: matchId,
        BetType: betType.value,
        BetName: betName
    };
    $.ajax({
        async: true,
        url: urlPrefix() + '/Odds/GetOddsComparison',
        data: JSON.stringify(oddsComparisonRequest),
        type: 'post',
        dataType: 'json',
        contentType: 'application/json; charset=utf-8',
        success: function (oddsComparisonPartial) {
            $("#oddsComparison").html(oddsComparisonPartial);
        },
        error: function (ex) {
            console.log(ex);
            $("#oddsComparison").html(ex.responseText);
        }
    });
}

function GetMatchByDate(searchDate, leagueId, btnSelected) {
    ShowLoadingSpinner('#listMatches');
    $(".btn-datebar").each(function () { $(this).removeClass("active"); });
    $("#btnLive").removeClass("active");
    console.log(btnSelected);
    $(btnSelected).addClass("active");
    $.ajax({
        async: true,
        url: urlPrefix() + '/Match/GetMatchesByDate?searchDate=' + searchDate + "&leagueId=" + leagueId,
        type: 'get',
        contentType: 'application/json; charset=utf-8',
        success: function (oddsComparisonPartial) {
            $("#listMatches").html(oddsComparisonPartial);
            HideLoadingSpinner('#listMatches');
            TriggerMatchDatatable();
        },
        error: function (ex) {
            console.log(ex);
            $("#listMatches").html(ex.responseText);
            HideLoadingSpinner('#listMatches');
            TriggerMatchDatatable();
        }
    });
}

function GetLiveMatch() {
    ShowLoadingSpinner('#listMatches');
    console.log($(".btn-datebar"));
    $(".btn-datebar").each(function () { $(this).removeClass("active"); });
    $("#btnLive").addClass("active");
    $.ajax({
        async: true,
        url: urlPrefix() + '/Match/GetLiveMatches',
        type: 'get',
        contentType: 'application/json; charset=utf-8',
        success: function (oddsComparisonPartial) {
            $("#listMatches").html(oddsComparisonPartial);
            HideLoadingSpinner('#listMatches');
            $('#btnCreateLiveMatches').click(function (event) {
                ShowLoadingSpinner('#listMatches');
                SendCreateLiveMatches();
            });
            TriggerMatchDatatable();
        },
        error: function (ex) {
            console.log(ex);
            $("#listMatches").html(ex.responseText);
            HideLoadingSpinner('#listMatches');
            $('#btnCreateLiveMatches').click(function (event) {
                ShowLoadingSpinner('#listMatches');
                SendCreateLiveMatches();
            });
            TriggerMatchDatatable();
        }
    });
}

function CloseLiveMatch(matchId) {
    ShowLoadingSpinner('#listMatches');
    $.ajax({
        async: true,
        url: urlPrefix() + '/Match/CloseLiveMatch?matchId=' + matchId,
        type: 'post',
        contentType: 'application/json; charset=utf-8',
        success: function (oddsComparisonPartial) {
            HideLoadingSpinner('#listMatches');
        },
        error: function (ex) {
            console.log(ex);
            HideLoadingSpinner('#listMatches');
        }
    });
}
function OpenLiveMatchDetail(matchId) {
    ShowLoadingSpinner('#listMatches');
    $.ajax({
        async: true,
        url: urlPrefix() + '/Live/PreviewLiveMatch?matchId=' + matchId,
        type: 'post',
        contentType: 'application/json; charset=utf-8',
        success: function (oddsComparisonPartial) {
            $("#listMatches").html(oddsComparisonPartial);
            HideLoadingSpinner('#listMatches');
        },
        error: function (ex) {
            console.log(ex);
            $("#listMatches").html(ex.responseText);
            HideLoadingSpinner('#listMatches');
        }
    });
}
function TriggerMatchDatatable() {
    var m = $('#mathes-table').DataTable({
        "columnDefs": [{
            "searchable": true,
            "orderable": true,
            "targets": 0
        }],
        "dom": '<"top"if>rt<"bottom"><"clear">',
        iDisplayLength: -1
    });

    m.on('order.dt search.dt', function () {
        m.column(0, { search: 'applied', order: 'applied' }).nodes().each(function (cell, i) {
            cell.innerHTML = i + 1;
        });
    }).draw();

    $('#mathes-table tbody').on('click', 'tr.group', function () {
        var currentOrder = table.order()[0];
        if (currentOrder[0] === groupColumn && currentOrder[1] === 'asc') {
            table.order([groupColumn, 'desc']).draw();
        }
        else {
            table.order([groupColumn, 'asc']).draw();
        }
    });
    var lm = $('#live-mathes-table').DataTable({
        "columnDefs": [{
            "searchable": true,
            "orderable": true,
            "targets": 0
        }],
        "order": [[3, 'asc'], [1, 'asc']],
        "dom": '<"top"if>rt<"bottom"><"clear">',
        iDisplayLength: -1
    });

    lm.on('order.dt search.dt', function () {
        m.column(0, { search: 'applied', order: 'applied' }).nodes().each(function (cell, i) {
            cell.innerHTML = i + 1;
        });
    }).draw();
}

function ShowLoadingSpinner(divId) {
    $('#loadingDiv').show();
    $(divId).hide();
}

function HideLoadingSpinner(divId) {
    $('#loadingDiv').hide();
    $(divId).show();
}
function ShowHideSearchDate() {
    if ($('#dateRangeSearch').hasClass("d-none")) {
        $('#dateRangeSearch').removeClass("d-none");
    } else {
        $('#dateRangeSearch').addClass("d-none");
    }
}

function UpdateLiveMatch(matchId, e) {
    e.preventDefault();
    var formInputs = e.target.elements;
    var sport_Event_Status = {
        status: formInputs["sport_event_status.status"].value,
        match_status: formInputs["sport_event_status.match_status"].value,
        home_score: formInputs["sport_event_status.home_score"].value,
        away_score: formInputs["sport_event_status.away_score"].value,
        winner_id: formInputs["sport_event_status.winner_id"].value,
        aggregate_home_score: formInputs["sport_event_status.aggregate_home_score"].value,
        aggregate_away_score: formInputs["sport_event_status.aggregate_away_score"].value,
        aggregate_winner_id: formInputs["sport_event_status.aggregate_winner_id"].value,
        period: formInputs["sport_event_status.period"].value,
        clock: {
            match_time: formInputs["sport_event_status.clock.match_time"].value
        },
        period_scores: null
    };
    console.log(sport_Event_Status);
    ShowLoadingSpinner('#listMatches');
    $.ajax({
        async: true,
        url: urlPrefix() + '/Live/UpdateLiveMatch?matchId=' + matchId,
        type: 'post',
        data: JSON.stringify(sport_Event_Status),
        dataType: 'json',
        contentType: 'application/json; charset=utf-8',
        success: function (oddsComparisonPartial) {
            GetLiveMatch();
        },
        error: function (ex) {
            console.log(ex);
            GetLiveMatch();
        }
    });
}