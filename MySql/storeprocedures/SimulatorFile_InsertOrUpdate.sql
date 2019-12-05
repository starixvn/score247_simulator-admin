DROP procedure IF EXISTS `SimulatorFile_InsertOrUpdate`;

CREATE DEFINER=`dev`@`%` PROCEDURE `SimulatorFile_InsertOrUpdate`(IN filePath VARCHAR(255), IN fileConent mediumtext, IN environment VARCHAR(45))
BEGIN
	INSERT INTO `SimulatorFile`(`Path`,`Value`, `Environment`)
			VALUES (
				filePath,
				JSON_UNQUOTE(JSON_EXTRACT(fileConent, '$')),
				environment)
			ON DUPLICATE KEY UPDATE
				`Value` = JSON_UNQUOTE(JSON_EXTRACT(fileConent, '$'));
END