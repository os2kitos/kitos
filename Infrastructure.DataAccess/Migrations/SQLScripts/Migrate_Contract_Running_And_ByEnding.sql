
/*
Content:
    We change the strings to enums as part of https://os2web.atlassian.net/browse/KITOSUDV-2258
    As they are used as enums, we change the model to match
*/

BEGIN
    UPDATE ItContract
    SET Running = 0
    WHERE Running = 'calendarYear'

    UPDATE ItContract
    SET ByEnding = 0
    WHERE ByEnding = 'calendarYear'

    UPDATE ItContract
    SET Running = 1
    WHERE Running = 'quater'

    UPDATE ItContract
    SET ByEnding = 1
    WHERE ByEnding = 'quater'

    UPDATE ItContract
    SET Running = 2
    WHERE Running = 'month'

    UPDATE ItContract
    SET ByEnding = 2
    WHERE ByEnding = 'month'

    UPDATE ItContract
    SET Running = null
    WHERE Running != 0 OR Running != 1 OR Running != 2 

    UPDATE ItContract
    SET ByEnding = null
    WHERE ByEnding != 0 OR ByEnding != 1 OR ByEnding != 2 
END