DROP procedure IF EXISTS `SimulatorFile_CleanFiles`;

CREATE DEFINER=`dev`@`%` PROCEDURE `SimulatorFile_CleanFiles`(IN environment VARCHAR(45))
BEGIN
	SET SQL_SAFE_UPDATES=0;
	DELETE FROM `SimulatorFile` as SF WHERE SF.Environment = environment;
	SET SQL_SAFE_UPDATES=1;
END