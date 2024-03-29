﻿/*
Content:
    Some advices with scheduling IS NULL are setup in hangfire with daily recurring job. These are first changed to have scheduling=2 (day)

    Updates existing Advice with AdviceType based on previous scheduling value:
    - All advices with scheduling=0 (Immediate) get AdviceType=0 (Immediate)
    - All advices with scheduling!=0 get AdviceType=1 (Repeat)
    - All advices with scheduling IS NULL get AdviceType=0 (Immediate)
*/

BEGIN

    UPDATE [Advice]
    SET Scheduling=2
    WHERE Id = 1145 OR Id = 1146 OR Id = 1192 OR Id = 1213 OR Id = 1882;
    
    UPDATE [Advice]
    SET AdviceType = 0
    WHERE Scheduling = 0;

    UPDATE [Advice]
    SET AdviceType = 1
    WHERE Scheduling is not NULL and Scheduling <> 0; 

    UPDATE [Advice]
    SET 
        AdviceType = 0, 
        Scheduling = 0
    WHERE Scheduling IS NULL;

    UPDATE [Advice]
    SET IsActive = 0
    WHERE IsActive = 1 and AdviceType = 0 and AlarmDate is NULL and StopDate is NULL;

END