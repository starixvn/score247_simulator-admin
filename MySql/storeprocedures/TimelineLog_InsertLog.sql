DROP procedure IF EXISTS `TimelineLog_InsertLog`;

CREATE DEFINER=`user`@`%` PROCEDURE `TimelineLog_InsertLog`(IN id bigint(20), IN matchId varchar(45), IN requestBody MEDIUMTEXT, IN timelineId bigint(20), IN isTimeline tinyint)
BEGIN
	IF(isTimeline IS NULL) THEN
		SET isTimeline := 1;
	END IF;
	INSERT INTO `TimelineLog`
	(`Id`, `MatchId`, `Value`, `TimelineId`, `IsTimeline`)
        VALUE (id, matchId, requestBody, timelineId, isTimeline)
	ON DUPLICATE KEY UPDATE
		`MatchId` = matchId,
        `Value` = requestBody,
        `TimelineId` = timelineId,
        `IsTimeline` = isTimeline;
END