DROP procedure IF EXISTS `Team_GetById`;

CREATE DEFINER=`dev`@`%` PROCEDURE `Team_GetById`(IN teamId varchar(255))
BEGIN
	SELECT * FROM `Team` WHERE `Id` = teamId
		LIMIT 1;
END