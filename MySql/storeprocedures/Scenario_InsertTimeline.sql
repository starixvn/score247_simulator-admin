DROP procedure IF EXISTS `Scenario_InsertTimeline`;

CREATE DEFINER=`user`@`%` PROCEDURE `Scenario_InsertTimeline`(IN scenarioId bigint, IN id bigint, in eventOrder int, in eventRequest text)
BEGIN
	INSERT INTO `Scenarios_TimelineLog`
		(`Id`, `ScenarioId`, `Order`, `EventRequest`)
        VALUE (id, scenarioId, eventOrder, JSON_EXTRACT(eventRequest, '$'));
END