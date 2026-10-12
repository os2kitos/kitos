-- PostgreSQL: read-only report, one row per organization.
-- Includes all existing records regardless of active/expired status.
-- Last change is MAX("LastChanged") among existing records; excludes deletions.
-- No records means a zero count and NULL date.
WITH usage_summary AS
         (
             SELECT "OrganizationId", COUNT(*) AS record_count,
                    MAX("LastChanged") AS last_change
             FROM dbo."ItSystemUsage"
             GROUP BY "OrganizationId"
         ),
     contract_summary AS
         (
             SELECT "OrganizationId", COUNT(*) AS record_count,
                    MAX("LastChanged") AS last_change
             FROM dbo."ItContract"
             GROUP BY "OrganizationId"
         ),
     dpr_summary AS
         (
             SELECT "OrganizationId", COUNT(*) AS record_count,
                    MAX("LastChanged") AS last_change
             FROM dbo."DataProcessingRegistrations"
             GROUP BY "OrganizationId"
         )
SELECT
    o."Uuid" AS "Organization Uuid",
    o."Name" AS "Organization Name",
    COALESCE(u.record_count, 0) AS "Number of usages",
    u.last_change AS "Last usage change",
    COALESCE(c.record_count, 0) AS "Number of contracts",
    c.last_change AS "Last contract change",
    COALESCE(d.record_count, 0) AS "Number of DPRs",
    d.last_change AS "Last DPR change"
FROM dbo."Organization" AS o
         LEFT JOIN usage_summary AS u ON u."OrganizationId" = o."Id"
         LEFT JOIN contract_summary AS c ON c."OrganizationId" = o."Id"
         LEFT JOIN dpr_summary AS d ON d."OrganizationId" = o."Id"
ORDER BY COALESCE(u.record_count, 0) DESC, o."Name", o."Uuid";
