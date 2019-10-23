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
            //$("#lineupsRequestContent").text(JSON.stringify(lineupsDto, null, '\t'));
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

function getRootUrl() {
    return window.location.origin
        ? window.location.origin + '/'
        : window.location.protocol + '/' + window.location.host + '/';
}

function urlPrefix() {
    return getRootUrl() + $('#urlPrefix').val();
}