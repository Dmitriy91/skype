USE [D:\STEP ACEDAMY\C#\MYWPFPROJECTSDBOPRODUCT\SERVER\DAO\SKYPEDATABASE.MDF]
GO

/****** Object:  StoredProcedure [dbo].[SP_Authenticate]    Script Date: 09.08.2015 23:36:52 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE PROC [dbo].[SP_Authenticate] @Login nvarchar(30), @Password char(64)
AS
BEGIN
	DECLARE @TempLogin nvarchar(30);
	--Login existence check
	IF NOT EXISTS(SELECT [Login] FROM [dbo].[User] WHERE [Login] = @Login) 
		RETURN 0;
	
	--Password existence check
	IF NOT EXISTS(SELECT [Login] FROM [dbo].[User] WHERE [Login] = @Login AND [Password] = @Password) 
		RETURN -1;

	SELECT UserID, [Login], [Email], [ImageName]
	FROM [dbo].[User] AS u
	WHERE u.[Login] = @Login AND u.[Password] = @Password

	RETURN 1;
END

GO

