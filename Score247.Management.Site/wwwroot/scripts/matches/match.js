function OpenCreateLeagueFixtureModel(e) {
    e.preventDefault();
    $.ajax({
        type: "GET",
        url: urlPrefix() + '/League/GetActives',
        contentType: "application/json; charset=utf-8",
        success: function (leagues) {
            $("#LeagueFixtureSelect").text = "";
            for (var i = 0; i < leagues.length; i++) {
                var opt = $("<option>").val(leagues[i].id).text(leagues[i].country + " " + leagues[i].name);
                $("#LeagueFixtureSelect").append(opt);
            }
            $('#CreateLeagueFixtureModal').modal();
        },
        error: function () {
            console.log("failed");
        }
    });
}

function GenerateLeagueFixture(e) {
    e.preventDefault();
    $("#CreateFixtureSpinner").removeClass("d-none");
    var leagueId = $("#LeagueFixtureSelect").val();
    var tournamentScheduleDto = JSON.parse($("#LeagueFixtureCase").val());
    console.log(leagueId);
    $.ajax({
        type: "POST",
        url: urlPrefix() + '/League/GenerateFixture?leagueId=' + leagueId,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        data: JSON.stringify(tournamentScheduleDto),
        success: function () {
            $("#CreateFixtureSpinner").addClass("d-none");

            $("#CreateLeagueFixtureModal").modal("hide");
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