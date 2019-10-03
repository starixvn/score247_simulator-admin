-- Table: public."Timeline"

-- DROP TABLE public."Timeline";

 CREATE TABLE `MajorLeague` (
  `Id` VARCHAR(45) NOT NULL,
  `Name` VARCHAR(255) NULL,
  `Leaguecol` VARCHAR(45) NULL,
  `Order` INT NULL,
  `CategoryId` VARCHAR(45) NULL,
  `Country` VARCHAR(255) NULL,
  `Region` VARCHAR(45) NULL,
  `IsActive` TINYINT NULL DEFAULT '0',
  `CountryCode` VARCHAR(45) NULL,
  `Language` VARCHAR(45) NOT NULL DEFAULT 'en-US',
  PRIMARY KEY (`Id`, `Language`));