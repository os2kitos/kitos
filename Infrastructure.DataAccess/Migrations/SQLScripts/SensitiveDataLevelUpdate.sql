BEGIN
INSERT INTO [ItSystemUsageSensitiveDataLevels] (ItSystemUsage_Id, SensitivityDataLevel)
SELECT Id, DataLevel
FROM [ItSystemUsage]

INSERT INTO [ItSystemUsageSensitiveDataLevels] (ItSystemUsage_Id, SensitivityDataLevel)
SELECT Id, 3
FROM [ItSystemUsage]
WHERE ContainsLegalInfo = 1
END
