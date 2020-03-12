/*
Content:
    Removes orphaned attached option registrations - all of type regularpersondata ([OptionType] = 0) and all related to itsystem ([ObjectType] = 0)
*/

BEGIN
    DELETE
    FROM [AttachedOptions] 
    WHERE [OptionType] = 0 OR [ObjectType] = 0;
END