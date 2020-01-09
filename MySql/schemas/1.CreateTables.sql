--
-- Table structure for table `Images`
--

DROP TABLE IF EXISTS `Images`;
CREATE TABLE `Images` (
  `Id` varchar(255) NOT NULL,
  `Name` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
  `CountryCode` varchar(45) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
  `Country3Code` varchar(45) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL ,
  `Flag` varchar(255) NULL DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Table structure for table `LeagueCategory`
--

DROP TABLE IF EXISTS `LeagueCategory`;
CREATE TABLE `LeagueCategory` (
  `Id` varchar(255) NOT NULL,
  `Name` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
  `CountryCode` varchar(45) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
  `Country3Code` varchar(45) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL ,
  `Flag` varchar(255) NULL DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Table structure for table `LeagueSeason`
--

DROP TABLE IF EXISTS `LeagueSeason`;
CREATE TABLE `LeagueSeason` (
  `Id` varchar(255) NOT NULL,
  `Name` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
  `StartDate` DateTime NULL,
  `EndDate` DateTime NULL ,
  `Year` varchar(45) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL DEFAULT NULL,
  `TournamentId` varchar(45) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL ,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;


--
-- Table structure for table `Scenarios`
--

DROP TABLE IF EXISTS `Scenarios`;
CREATE TABLE `Scenarios` (
  `Id` bigint(20) NOT NULL,
  `Name` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
  `HomeId` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
  `AwayId` DateTime NULL ,
  `EventDate` DateTime NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Table structure for table `Scenarios_TimelineLog`
--

DROP TABLE IF EXISTS `Scenarios_TimelineLog`;
CREATE TABLE `Scenarios_TimelineLog` (
  `Id` bigint(20) NOT NULL,
  `EventRequest` json NULL,
  `Order` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
  `ScenarioId` bigint(20) NULL ,
  `IsTimeline` tinyint(4) NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;


--
-- Table structure for table `Scenarios_TimelineLog`
--

DROP TABLE IF EXISTS `SimulatorFile`;
CREATE TABLE `SimulatorFile` (
  `Path` varchar(255) NOT NULL,
  `Environment` varchar(45) NULL,
  `Value` JSON NULL,
  PRIMARY KEY (`Path`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;


--
-- Table structure for table `TimelineLog`
--

DROP TABLE IF EXISTS `TimelineLog`;
CREATE TABLE `TimelineLog` (
  `Id` bigint(20) NOT NULL,
  `MatchId` varchar(45) NULL,
  `Value` JSON NULL,
  `TimelineId` bigint(20) NULL,
  `IsTimeline` tinyint(4) NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

