DROP procedure IF EXISTS `League_CreateLeagueSeason`;

CREATE DEFINER=`user`@`%` PROCEDURE `League_CreateLeagueSeason`(
		IN seasonId VARCHAR(45), 
        IN seasonName varchar(255),
        IN startDate Datetime,
        IN endDate Datetime,
        IN seasonYear varchar(45),
        IN tournamentId varchar(45))
BEGIN
	if not exists (select * from `LeagueSeason` where `Id` = seasonId) then
		INSERT INTO `LeagueSeason`
			(`Id`, `Name`, `StartDate`, `EndDate`, `Year`, `TournamentId`)
			VALUE (seasonId, seasonName, startDate, endDate, seasonYear, tournamentId);
	end if;
END