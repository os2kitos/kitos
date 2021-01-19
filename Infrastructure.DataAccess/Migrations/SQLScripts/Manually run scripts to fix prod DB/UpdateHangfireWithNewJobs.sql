INSERT INTO
	[kitos_HangfireDB].[HangFire].[Set] ([Key], Score, [Value], ExpireAt)

SELECT  [Key], Score, NewJobId, ExpireAt
FROM [kitos_HangfireDB].[HangFire].[Set]
INNER JOIN
	(SELECT OldJobIds.JobId, NewJobIds.JobId 
	FROM 
	(
		SELECT s.* 
		FROM [kitos].[dbo].[Advice] s 
		JOIN
			(
			SELECT Name, Subject, Body, Scheduling, SentDate
			FROM kitos.dbo.Advice
			WHERE Type = 0 OR Type = 4
			GROUP BY Name, Subject, Body, Scheduling, SentDate
			HAVING COUNT(*) > 1
			) AS k
		ON 
			((s.Name IS NULL AND k.Name IS NULL) OR s.Name = k.Name)
			AND
			((s.Subject IS NULL AND k.Subject IS NULL) OR s.Subject = k.Subject)
			AND
			((s.Body IS NULL AND k.Body IS NULL) OR s.Body = k.Body)
			AND
			((s.Scheduling IS NULL AND k.Scheduling IS NULL) OR s.Scheduling = k.Scheduling)
			AND
			((s.SentDate IS NULL AND k.SentDate IS NULL) OR s.SentDate = k.SentDate)
		WHERE s.Type = 0
	) AS OldJobIds
	INNER JOIN
	(
		SELECT s.* 
		FROM [kitos].[dbo].[Advice] s 
		JOIN
			(
			SELECT Name, Subject, Body, Scheduling, SentDate
			FROM kitos.dbo.Advice
			WHERE Type = 0 OR Type = 4
			GROUP BY Name, Subject, Body, Scheduling, SentDate
			HAVING COUNT(*) > 1
			) AS k
		ON 
			((s.Name IS NULL AND k.Name IS NULL) OR s.Name = k.Name)
			AND
			((s.Subject IS NULL AND k.Subject IS NULL) OR s.Subject = k.Subject)
			AND
			((s.Body IS NULL AND k.Body IS NULL) OR s.Body = k.Body)
			AND
			((s.Scheduling IS NULL AND k.Scheduling IS NULL) OR s.Scheduling = k.Scheduling)
			AND
			((s.SentDate IS NULL AND k.SentDate IS NULL) OR s.SentDate = k.SentDate)
		WHERE s.Type = 4
	) AS NewJobIds ON 
		((OldJobIds.Name IS NULL AND NewJobIds.Name IS NULL) OR OldJobIds.Name = NewJobIds.Name)
		AND 
		((OldJobIds.Subject IS NULL AND NewJobIds.Subject IS NULL) OR OldJobIds.Subject = NewJobIds.Subject)
		AND 
		((OldJobIds.Scheduling IS NULL AND NewJobIds.Scheduling IS NULL) OR OldJobIds.Scheduling = NewJobIds.Scheduling)
		AND 
		((OldJobIds.Body IS NULL AND NewJobIds.Body IS NULL) OR OldJobIds.Body = NewJobIds.Body)
		AND 
		((OldJobIds.SentDate IS NULL AND NewJobIds.SentDate IS NULL) OR OldJobIds.SentDate = NewJobIds.SentDate)
	) AS JobIdsToCopy (OldJobId, NewJobId) ON JobIdsToCopy.OldJobId = [kitos_HangfireDB].[HangFire].[Set].Value


