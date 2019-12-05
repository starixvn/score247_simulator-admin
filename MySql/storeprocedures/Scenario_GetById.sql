DROP procedure IF EXISTS `Scenario_GetById`;

CREATE DEFINER=`user`@`%` PROCEDURE `Scenario_GetById`(IN id bigint)
BEGIN
	SELECT * FROM `Scenarios` as sc where sc.Id = id;
END