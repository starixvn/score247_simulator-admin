DROP procedure IF EXISTS `Team_GetTeam`;

CREATE DEFINER=`user`@`%` PROCEDURE `Team_GetTeam`()
BEGIN
	SELECT *
		FROM `Team`;
END