function generateLeagueInfoData(leagueId, e) {
    e.preventDefault();
    $.ajax({
        type: 'GET',
        url: urlPrefix() + `/League/DumpLeagueInfo?leagueId=${leagueId}`,
        success: function (leagueInfo) {
            $("#leagueDataRaw").val(JSON.stringify(leagueInfo, null, '\t'));
            $("#leagueRequestContent").text("");
            let target = '#leagueRequestContent';
            jsonView.format(leagueInfo, target);
            $('#leagueInfoRequest').modal();
        },
        error: function (error) {
            console.log("failed" + JSON.stringify(error) + urlPrefix() + `/League/DumpLeagueInfo?leagueId=${leagueId}`);
        }
    });
}

function updateLeagueInfoData(e) {
    e.preventDefault();
    var leagueSportradarInfo = JSON.parse($("#leagueDataRaw").val());
    console.log(leagueSportradarInfo);
    $.ajax({
        type: "POST",
        url: urlPrefix() + "/League/UpdateLeagueInfo",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        data: JSON.stringify(leagueSportradarInfo),
        success: function () {
            console.log("success");
        },
        error: function () {
            console.log("failed");
        }
    });
}

function FormatJsonRawData(rawDataTextarea, targetDiv) {
    var jsonRaw = $("#" + rawDataTextarea).val();
    var jsonObj = JSON.parse(jsonRaw);
    console.log(jsonObj);
    $("#" + targetDiv).text("");
    let target = '#' + targetDiv;
    jsonView.format(jsonObj, target);
}

function getRootUrl() {
    return window.location.origin
        ? window.location.origin + '/'
        : window.location.protocol + '/' + window.location.host + '/';
}

function urlPrefix() {
    return getRootUrl() + $('#urlPrefix').val();
}