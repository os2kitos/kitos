/* KITOSUDV-1286 Copy advice back to contract module so they are present both in contracts and DPR modules */

DECLARE @ContractAdvice TABLE 
    (
		AdviceId int,
		OrigJobId nvarchar(max),
		NewJobId nvarchar(max)
    )

INSERT INTO kitosProd.dbo.Advice (IsActive, Name, AlarmDate, SentDate, ReceiverId, CarbonCopyReceiverId, Subject, ObjectOwnerId, LastChanged, LastChangedByUserId, RelationId, Type, Scheduling, StopDate, Body, JobId)
OUTPUT inserted.Id, inserted.JobId INTO @ContractAdvice (AdviceId, OrigJobId)
SELECT IsActive, Name, AlarmDate, SentDate, ReceiverId, CarbonCopyReceiverId, Subject, ObjectOwnerId, LastChanged, LastChangedByUserId, RelationId, Type, Scheduling, StopDate, Body, JobId
	FROM kitos.dbo.Advice
	WHERE Type = 0
		AND Id IN (SELECT Id FROM kitosProd.dbo.Advice WHERE Type = 4)


UPDATE kitosProd.dbo.Advice
SET JobId = 'Advice: ' + CONVERT(varchar(MAX), Advice.Id)
WHERE Id IN (SELECT AdviceId FROM @ContractAdvice)

UPDATE @ContractAdvice
SET NewJobId = JobId
FROM @ContractAdvice
	INNER JOIN
	kitosProd.dbo.Advice ON Id = AdviceId


INSERT INTO [kitos_HangfireDBProd].[HangFire].[Set] ([Key], Score, [Value], ExpireAt)
SELECT [Key], Score, NewJobId, ExpireAt
FROM [kitos_HangfireDB].[HangFire].[Set]
	INNER JOIN 
	@ContractAdvice ON [Value] = OrigJobId
WHERE Value IN(
	SELECT JobId
	FROM kitos.dbo.Advice
	WHERE Type = 0
		AND Id IN (SELECT Id FROM kitosProd.dbo.Advice WHERE Type = 4)
	)

