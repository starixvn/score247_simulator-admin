DROP procedure IF EXISTS `League_CreateLeagueCategory`;

CREATE DEFINER=`user`@`%` PROCEDURE `League_CreateLeagueCategory`(
		IN categoryId VARCHAR(45), 
        IN categoryName varchar(255),
        IN categoryCode varchar(45))
BEGIN
	if not exists (select * from `LeagueCategory` where `Id` = categoryId) then
		INSERT INTO `LeagueCategory`
				(`Id`, `Name`, `CountryCode`)
				VALUE (categoryId, categoryName, categoryCode);
	end if;
END