/*
User story reference: 
    https://os2web.atlassian.net/browse/KITOSUDV-3402
 
Content:
    
*/

BEGIN
    SELECT DISTINCT T0.Id
    FROM [User] T0
        LEFT JOIN  OrganizationRights T1
        ON T0.Id = T1.UserId
    WHERE T0.Deleted = 0
        AND (T1.Role = 1
        OR T1.Role = 7
        OR T0.HasApiAccess = 1 AND T1.UserId IS NOT NULL)
END