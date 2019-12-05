DROP procedure IF EXISTS `League_GetCategory`;

CREATE DEFINER=`user`@`%` PROCEDURE `League_GetCategory`()
BEGIN
	SELECT *
		FROM `LeagueCategory`;
END