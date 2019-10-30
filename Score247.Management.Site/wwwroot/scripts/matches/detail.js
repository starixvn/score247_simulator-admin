function generateLineupsData(matchId, e) {
    e.preventDefault();
    var values = $("#generateLineupsData").serialize();
    console.log(values);
    $.ajax({
        type: "POST",
        url: urlPrefix() + '/Match/Lineups?matchId=' + matchId + '&' + values,
        contentType: "application/json; charset=utf-8",
        success: function (lineupsDto) {
            $('#btnApplyLineups').on("click", function () {
                console.log("ASdasdads");
                applyLineups(matchId, lineupsDto);
            });
            $("#lineupsRequestContent").text("");
            let target = '#lineupsRequestContent';
            jsonView.format(lineupsDto, target);
            $('#lineupsRequest').modal();
        },
        error: function () {
            console.log("failed");
        }
    });
}

function applyLineups(matchId, lineUpsDto) {
    $.ajax({
        type: "POST",
        url: urlPrefix() + '/Match/Lineups/' + matchId,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        data: JSON.stringify(lineUpsDto),
        success: function () {
            console.log("success");
        },
        error: function () {
            console.log("failed");
        }
    });
}

function getMatchLineups(matchId, e) {
    e.preventDefault();
    $.ajax({
        type: "GET",
        url: urlPrefix() + '/Match/Lineups/' + matchId,
        contentType: "application/json; charset=utf-8",
        success: function (lineUpData) {
            $("#lineupsDataContent").text("");
            document.getElementById('lineupsImage').innerHTML = lineUpData.pitchView;
            lineUpData.pitchView = null;
            let target = '#lineupsDataContent';
            jsonView.format(lineUpData, target);
            $('#lineupsData').modal();
        },
        error: function () {
            $("#lineupsDataContent").text("No Data");
            $('#lineupsData').modal();
        }
    });
}

function generateH2HData(matchId, e) {
    e.preventDefault();
    console.log(matchId);
    var values = $("#generateH2HData").serialize();
    $.ajax({
        type: "POST",
        url: urlPrefix() + '/Team/H2H/Generate?matchId=' + matchId + '&' + values,
        contentType: "application/json; charset=utf-8",
        success: function (h2HDto) {
            $('#btnApplyH2H').on("click", function () {
                applyH2H(matchId, h2HDto);
            });
            $("#H2HRequestContent").text("");
            let target = '#H2HRequestContent';
            jsonView.format(h2HDto, target);
            $('#h2hRequest').modal();
        },
        error: function () {
            console.log("failed");
        }
    });
}

function applyH2H(matchId, h2HDto) {
    $.ajax({
        type: "POST",
        url: urlPrefix() + '/Team/H2H/Apply/' + matchId,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        data: JSON.stringify(h2HDto),
        success: function () {
            console.log("success");
        },
        error: function () {
            console.log("failed");
        }
    });
}

function getMatchH2H(matchId, e) {
    e.preventDefault();
    $.ajax({
        type: "GET",
        url: urlPrefix() + '/Team/H2H/' + matchId,
        contentType: "application/json; charset=utf-8",
        success: function (h2hData) {
            $("#h2hDataContent").text("");
            let target = '#h2hDataContent';
            jsonView.format(h2hData, target);
            $('#h2hData').modal();
        },
        error: function () {
            $("#h2hDataContent").text("No Data");
            $('#h2hData').modal();
        }
    });
}

function generateTeamResultData(matchId, e) {
    e.preventDefault();
    var values = $("#generateTeamResultData").serialize();
    $.ajax({
        type: "POST",
        url: urlPrefix() + '/Team/Results/Generate?matchId=' + matchId + '&' + values,
        contentType: "application/json; charset=utf-8",
        success: function (teamResultDto) {
            $('#btnApplyH2H').on("click", function () {
                applyTeamResult(matchId, teamResultDto);
            });
            $("#H2HRequestContent").text("");
            let target = '#H2HRequestContent';
            jsonView.format(teamResultDto, target);
            $('#h2hRequest').modal();
        },
        error: function () {
            console.log("generate team result failed");
        }
    });
}

function applyTeamResult(matchId, teamResultDto) {
    $.ajax({
        type: "POST",
        url: urlPrefix() + '/Team/Results/Apply/' + matchId,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        data: JSON.stringify(teamResultDto),
        success: function () {
            console.log("success");
        },
        error: function () {
            console.log("apply team result failed");
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