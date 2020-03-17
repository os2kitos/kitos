BEGIN
INSERT INTO [dbo].[ItSystemUsageSensitiveDataLevels] (ItSystemUsage_Id, SensitivityDataLevel)
SELECT Id, DataLevel
FROM [dbo].[ItSystemUsage]

INSERT INTO [dbo].[ItSystemUsageSensitiveDataLevels] (ItSystemUsage_Id, SensitivityDataLevel)
SELECT Id, 3
FROM [dbo].[ItSystemUsage]
WHERE ContainsLegalInfo = 1
END
