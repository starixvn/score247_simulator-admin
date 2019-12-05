DROP procedure IF EXISTS `Scenario_Insert`;

CREATE DEFINER=`user`@`%` PROCEDURE `Scenario_Insert`(IN id bigint, IN name VARCHAR(255), IN homeId Varchar(45), IN awayId Varchar(45), IN eventDate datetime)
BEGIN
	INSERT INTO `Scenarios`
		(`Id`, `Name`, `HomeId`, `AwayId`, `EventDate`)
        VALUE (id, name, homeId, awayId, eventDate);
END