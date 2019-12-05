DROP procedure IF EXISTS `SimulatorFile_GetFile`;

CREATE DEFINER=`dev`@`%` PROCEDURE `SimulatorFile_GetFile`(IN filePath VARCHAR(255), IN environment VARCHAR(45))
BEGIN
	SELECT SF.Value FROM `SimulatorFile` as SF WHERE SF.Path = filePath AND SF.Environment = environment;
END