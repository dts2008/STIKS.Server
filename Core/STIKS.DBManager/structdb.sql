CREATE SCHEMA stiks;

USE stiks;

CREATE TABLE userinfo 
(
	`Id` int NOT NULL AUTO_INCREMENT,
	`Name` varchar(256),
    `Email` varchar(512),
    `Password` varchar(128) NOT NULL,
	PRIMARY KEY (`id`),
    CONSTRAINT userinfo_email UNIQUE (`Email`)
);
