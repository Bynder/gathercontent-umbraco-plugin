IF OBJECT_ID('dbo.[gcAccountSettings]', 'U') IS NOT NULL 
BEGIN
ALTER TABLE [dbo].[gcAccountSettings]
ADD ImportDateFormat nvarchar(255);
END