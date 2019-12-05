DROP procedure IF EXISTS `Team_InsertTeam`;

CREATE DEFINER=`user`@`%` PROCEDURE `Team_InsertTeam`(
	in id varchar(45),
    in abbreviation varchar(45),
    in country varchar(255),
    in countryCode varchar(45),
    in teamName varchar(255)
)
BEGIN
	INSERT INTO `Team`
		(`Id`, `Abbreviation`, `Country`, `CountryCode`, `Name`)
        VALUE (id, abbreviation, country, countryCode, teamName);
END