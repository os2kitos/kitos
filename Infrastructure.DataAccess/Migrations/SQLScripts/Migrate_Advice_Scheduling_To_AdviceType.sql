/*
Content:
    Updates existing Advice with AdviceType based on previous scheduling value:
    - All advices with scheduling=0 (Immediate) get AdviceType=0 (Immediate)
    - All advices with scheduling!=0 get AdviceType=1 (Repeat)
    - All advices with scheduling IS NULL get AdviceType=0 (Immediate)
*/

BEGIN
    
    UPDATE [kitos].[dbo].[Advice]
    SET AdviceType=0
    WHERE Scheduling=0

    UPDATE [kitos].[dbo].[Advice]
    SET AdviceType=1
    WHERE Scheduling!=0

    UPDATE [kitos].[dbo].[Advice]
    SET AdviceType=0
    WHERE Scheduling IS NULL

END