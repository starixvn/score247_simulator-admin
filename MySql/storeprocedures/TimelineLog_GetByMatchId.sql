DROP procedure IF EXISTS `TimelineLog_GetByMatchId`;

CREATE DEFINER=`user`@`%` PROCEDURE `TimelineLog_GetByMatchId`(IN id varchar(45))
BEGIN
	SELECT *
		FROM `TimelineLog`
        where MatchId = id;
END