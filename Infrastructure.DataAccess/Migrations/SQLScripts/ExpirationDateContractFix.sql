UPDATE [dbo].[ItContract]
  SET ExpirationDate = '2050-01-01'
  WHERE ExpirationDate > DATEADD(year,1000,GETDATE())