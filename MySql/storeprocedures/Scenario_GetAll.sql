DROP procedure IF EXISTS `Scenario_GetAll`;

CREATE DEFINER=`user`@`%` PROCEDURE `Scenario_GetAll`()
BEGIN
	SELECT * FROM `Scenarios`;
END