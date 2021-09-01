/*
Content:
    Removes orphaned attached option registrations - Any object type other than 1 or situations where the object has been deleted
*/

BEGIN
    DELETE 
    FROM [AttachedOptions] 
    WHERE (ObjectType <> 1) or (ObjectType = 1 and ObjectId not in (select Id from ItSystemUsage));
END