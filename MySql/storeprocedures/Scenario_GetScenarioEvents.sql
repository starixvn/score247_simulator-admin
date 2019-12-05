DROP procedure IF EXISTS `Scenario_GetScenarioEvents`;

CREATE DEFINER=`user`@`%` PROCEDURE `Scenario_GetScenarioEvents`(In scenarioId bigint)
BEGIN
	SELECT * FROM `Scenarios_TimelineLog` as ST where ST.ScenarioId = scenarioId order by ST.Order;
END