/*

Content:
	Migrates existing DPR oversight date data to a new list of dates as described in https://os2web.atlassian.net/browse/KITOSUDV-1307

*/

BEGIN
	INSERT INTO DataProcessingRegistrationOversightDates(ParentId, OversightRemark, OversightDate)
	SELECT Id, OversightCompletedRemark, LatestOversightDate
	FROM DataProcessingRegistrations
END