/*
User story reference: 
    https://os2web.atlassian.net/browse/KITOSUDV-1882
 
Content:
    Adds FK bindings to the different roles and fixes the old issue of losing track of roles because bindings were by name.
*/

---------------
--- SYSTEM ADVICES
---------------
UPDATE AdviceUserRelations
SET ItSystemRoleId = ItSystemRoles.Id

FROM
	AdviceUserRelations INNER JOIN
	ItSystemRoles ON AdviceUserRelations.Email = ItSystemRoles.Name INNER JOIN
	Advice ON AdviceUserRelations.AdviceId = Advice.Id
WHERE
	Advice.Type = 1 AND						--Itsystem Advice
	AdviceUserRelations.RecpientType = 0	--Role receiver

--------------------
--- CONTRACT ADVICES
--------------------
UPDATE AdviceUserRelations
SET ItContractRoleId = ItContractRoles.Id

FROM
	AdviceUserRelations INNER JOIN
	ItContractRoles ON AdviceUserRelations.Email = ItContractRoles.Name INNER JOIN
	Advice ON AdviceUserRelations.AdviceId = Advice.Id
WHERE
	Advice.Type = 0 AND						--ItContract Advice
	AdviceUserRelations.RecpientType = 0	--Role receiver

--------------------
--- PROJECT ADVICES
--------------------
UPDATE AdviceUserRelations
SET ItProjectRoleId = ItProjectRoles.Id

FROM
	AdviceUserRelations INNER JOIN
	ItProjectRoles ON AdviceUserRelations.Email = ItProjectRoles.Name INNER JOIN
	Advice ON AdviceUserRelations.AdviceId = Advice.Id
WHERE
	Advice.Type = 2 AND						--ITProject Advice
	AdviceUserRelations.RecpientType = 0	--Role receiver


---------------
--- DPR ADVICES
---------------
UPDATE AdviceUserRelations
SET DataProcessingRegistrationRoleId = DataProcessingRegistrationRoles.Id

FROM
	AdviceUserRelations INNER JOIN
	DataProcessingRegistrationRoles ON AdviceUserRelations.Email = DataProcessingRegistrationRoles.Name INNER JOIN
	Advice ON AdviceUserRelations.AdviceId = Advice.Id
WHERE
	Advice.Type = 4 AND						--DPR Advice
	AdviceUserRelations.RecpientType = 0	--Role receiver

-- Delete orphans
delete from dbo.[AdviceUserRelations] 
where 
    RecpientType = 0 and
    ItContractRoleId is null and 
    ItProjectRoleId is null and 
    ItSystemRoleId is null and 
    DataProcessingRegistrationRoleId is null;

-- Update email column on role fields
UPDATE dbo.AdviceUserRelations 
SET Email = NULL
WHERE RecpientType = 0;