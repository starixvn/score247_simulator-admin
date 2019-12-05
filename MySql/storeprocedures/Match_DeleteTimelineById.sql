DROP procedure IF EXISTS `Match_DeleteTimelineById`;

CREATE DEFINER=`root`@`%` PROCEDURE `Match_DeleteTimelineById`(IN timelineId bigint)
BEGIN
	DELETE FROM `TimelineLog` as TLL WHERE TLL.Id = timelineId;
END