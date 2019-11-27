function generateLineupsData(matchId, e) {
    e.preventDefault();
    var values = $("#generateLineupsData").serialize();
    console.log(values);
    $.ajax({
        type: "POST",
        url: urlPrefix() + '/Match/Lineups?matchId=' + matchId + '&' + values,
        contentType: "application/json; charset=utf-8",
        success: function (lineupsDto) {
            $("#lineupsDataRaw").val(JSON.stringify(lineupsDto, null, '\t'));
            $('#btnApplyLineups').on("click", function () {
                applyLineups(matchId);
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

function applyLineups(matchId) {
    var lineUpsDto = JSON.parse($("#lineupsDataRaw").val());
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
            $("#H2HRequestDataRaw").val(JSON.stringify(h2HDto, null, '\t'));
            $('#btnApplyH2H').on("click", function () {
                applyH2H(matchId);
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

function applyH2H(matchId) {
    var h2HDto = JSON.parse($("#H2HRequestDataRaw").val());
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
            $("#H2HRequestDataRaw").val(JSON.stringify(teamResultDto, null, '\t'));
            $('#btnApplyH2H').on("click", function () {
                applyTeamResult(matchId);
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

function applyTeamResult(matchId) {
    var teamResultDto = JSON.parse($("#H2HRequestDataRaw").val());
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

function scoreEventTypeChange(selectObject) {
    var scoreEventType = $('#scoreEventType').find('option:selected').val();
    scoreTeamChange(null);
    if (scoreEventType === 'normal_goal') {
        $('#scoreAssist').removeClass('d-none');
    } else {
        $('#scoreAssist').addClass('d-none');
    }
}

function scoreTeamChange(selectObject) {
    var scoreEventType = $('#scoreEventType').find('option:selected').val();
    var scoreEventTeam = $('#scoreEventTeam').find('option:selected').val();
    var shouldShowAway = scoreEventTeam === "away" && scoreEventType !== "own_goal" || scoreEventTeam === "home" && scoreEventType === "own_goal";
    console.log(shouldShowAway);
    if (shouldShowAway === true) {
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
    $('#scoreGoalScorerId').val(selectObject.value);
    $('#scoreGoalScorerName').val($(selectObject).find('option:selected').text());
}

function scoreAssistChange(selectObject) {
    console.log(selectObject);
    $('#scoreAssistId').val(selectObject.value);
    $('#scoreAssistName').val($(selectObject).find('option:selected').text());
}

function FormatJsonRawData(rawDataTextarea, targetDiv) {
    var jsonRaw = $("#" + rawDataTextarea).val();
    var jsonObj = JSON.parse(jsonRaw);
    console.log(jsonObj);
    $("#" + targetDiv).text("");
    let target = '#' + targetDiv;
    jsonView.format(jsonObj, target);
}

//league standing
function generateStandingData(matchId, e) {
    e.preventDefault();
    $.ajax({
        type: "GET",
        url: urlPrefix() + '/League/Standing?matchId=' + matchId,
        contentType: "application/json; charset=utf-8",
        success: function (tournamentStandingDto) {
            $("#leagueStandingDataRaw").val(JSON.stringify(tournamentStandingDto, null, '\t'));
            $('#btnApplyStanding').on("click", function () {
                applyStanding();
            });
            $("#leagueStandingContent").text("");
            let target = '#leagueStandingContent';
            jsonView.format(tournamentStandingDto, target);
            $('#leagueStandingRequest').modal();
        },
        error: function () {
            console.log("failed");
        }
    });
}

function applyStanding() {
    var tournamentStandingDto = JSON.parse($("#leagueStandingDataRaw").val());
    console.log(tournamentStandingDto);
    $.ajax({
        type: "POST",
        url: urlPrefix() + '/League/Standing/Apply',
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        data: JSON.stringify(tournamentStandingDto),
        success: function () {
            console.log("success");
        },
        error: function () {
            console.log("failed");
        }
    });
}

function generateEventData(matchId, formId, eventType, e) {
    e.preventDefault();
    var form = $(`#${formId}`).serialize();
    console.log(form);
    console.log(matchId);
    $.ajax({
        type: 'POST',
        url: urlPrefix() + `/Match/Create${eventType}Partial/${matchId}`,
        data: form,
        dataType: 'json',
        success: function (eventData) {
            $("#eventDataRaw").val(JSON.stringify(eventData, null, '\t'));
            $("#eventRequestContent").text("");
            let target = '#eventRequestContent';
            jsonView.format(eventData, target);
            $('#eventRequest').modal();
        },
        error: function (error) {
            console.log("failed" + error);
        }
    });
}

function applyEvent(missing) {
    var matchPushEventDto = JSON.parse($("#eventDataRaw").val());
    $.ajax({
        type: "POST",
        url: urlPrefix() + `/Match/Event/Apply?eventMissed=${missing}`,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        data: JSON.stringify(matchPushEventDto),
        success: function () {
            console.log("success");
            location.reload();
        },
        error: function () {
            console.log("failed");
        }
    });
}