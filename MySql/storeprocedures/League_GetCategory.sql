DROP procedure IF EXISTS `Odds_InsertOdds`;

CREATE DEFINER=`user`@`%` PROCEDURE `Odds_InsertOdds`(
		IN matchId VARCHAR(45), 
        IN oddsList TEXT)
BEGIN
		DECLARE oddsIndex INT DEFAULT 0;
		DECLARE totalOdds INT DEFAULT JSON_LENGTH(oddsList);
		
		WHILE oddsIndex < totalOdds DO       
        
        INSERT INTO `Odds`
						(`CreatedTime`,
						`Value`,
						`matchId`,
						`BetTypeId`,
						`BookmakerId`)
						VALUES
						(UTC_TIMESTAMP(6),
						JSON_EXTRACT(oddsList, CONCAT('$[', oddsIndex, ']')),
						matchId,
						JSON_EXTRACT(oddsList, CONCAT('$[', oddsIndex, '].Id')),
						JSON_UNQUOTE(JSON_EXTRACT(oddsList, CONCAT('$[', oddsIndex, '].Bookmaker.Id'))));
			-- Increment the loop variable                                                                                                                                                        
			SET oddsIndex = oddsIndex + 1;
		END WHILE;
	END