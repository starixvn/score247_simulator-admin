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

//Card Event Change
function cardTeamChange(selectObject) {
    if (selectObject.value === "away") {
        $('#cardHomePlayerOptions').addClass("d-none");
        $('#cardAwayPlayerOptions').removeClass("d-none");
        cardPlayerChange($('#cardAwayPlayerOptions'));
    } else {
        $('#cardAwayPlayerOptions').addClass("d-none");
        $('#cardHomePlayerOptions').removeClass("d-none");
        cardPlayerChange($('#cardHomePlayerOptions'));
    }
}

function cardPlayerChange(selectObject) {
    console.log(selectObject);
    $('#cardPlayerId').val(selectObject.value);
    $('#cardPlayerName').val($(selectObject).find('option:selected').text());
}

//Penalty Event Change
function penaltyTeamChange(selectObject) {
    if (selectObject.value === "away") {
        $('#penaltyHomeGoalScorerOptions').addClass("d-none");
        $('#penaltyAwayGoalScorerOptions').removeClass("d-none");
        penaltyGoalScorerChange($('#penaltyAwayGoalScorerOptions'));
    } else {
        $('#penaltyAwayGoalScorerOptions').addClass("d-none");
        $('#penaltyHomeGoalScorerOptions').removeClass("d-none");
        penaltyGoalScorerChange($('#penaltyHomeGoalScorerOptions'));
    }
}

function penaltyGoalScorerChange(selectObject) {
    console.log(selectObject);
    $('#penaltyGoalScorerId').val(selectObject.value);
    $('#penaltyGoalScorerName').val($(selectObject).find('option:selected').text());
}

//Substitute Event Change
function substituteTeamChange(selectObject) {
    if (selectObject.value === "away") {
        $('#subHomePlayerInOptions').addClass("d-none");
        $('#subAwayPlayerInOptions').removeClass("d-none");
        subPlayerInChange($('#subAwayPlayerInOptions'));
        $('#subHomePlayerOutOptions').addClass("d-none");
        $('#subAwayPlayerOutOptions').removeClass("d-none");
        subPlayerOutChange($('#subAwayPlayerOutOptions'));
    } else {
        $('#subAwayPlayerInOptions').addClass("d-none");
        $('#subHomePlayerInOptions').removeClass("d-none");
        subPlayerInChange($('#subHomePlayerInOptions'));
        $('#subAwayPlayerOutOptions').addClass("d-none");
        $('#subHomePlayerOutOptions').removeClass("d-none");
        subPlayerOutChange($('#subHomePlayerOutOptions'));
    }
}

function subPlayerInChange(selectObject) {
    console.log(selectObject);
    $('#subPlayerInId').val(selectObject.value);
    $('#subPlayerInName').val($(selectObject).find('option:selected').text());
}

function subPlayerOutChange(selectObject) {
    console.log(selectObject);
    $('#subPlayerOutId').val(selectObject.value);
    $('#subPlayerOutName').val($(selectObject).find('option:selected').text());
}

//Score Event Change
function scoreTeamChange(selectObject) {
    if (selectObject.value === "away") {
        $('#scoreHomeScorerOptions').addClass("d-none");
        $('#scoreAwayScorerOptions').removeClass("d-none");
        scoreScorerChange($('#scoreAwayScorerOptions'));

        $('#scoreHomeAssistOptions').addClass("d-none");
        $('#scoreAwayAssistOptions').removeClass("d-none");
        scoreAssistChange($('#scoreAwayAssistOptions'));
    } else {
        $('#scoreAwayScorerOptions').addClass("d-none");
        $('#scoreHomeScorerOptions').removeClass("d-none");
        scoreScorerChange($('#scoreHomeScorerOptions'));

        $('#scoreAwayAssistOptions').addClass("d-none");
        $('#scoreHomeAssistOptions').removeClass("d-none");
        scoreAssistChange($('#scoreHomeAssistOptions'));
    }
}

function scoreScorerChange(selectObject) {
    console.log(selectObject);
    $('#scoreGoalScorerId').val(selectObject.value);
    $('#scoreGoalScorerName').val($(selectObject).find('option:selected').text());
}

function scoreAssistChange(selectObject) {
    console.log(selectObject);
    $('#scoreAssistId').val(selectObject.value);
    $('#scoreAssistName').val($(selectObject).find('option:selected').text());
}