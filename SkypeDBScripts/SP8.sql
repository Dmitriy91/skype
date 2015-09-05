USE [D:\STEP ACEDAMY\C#\MYWPFPROJECTSDBOPRODUCT\SERVER\DAO\SKYPEDATABASE.MDF]
GO

/****** Object:  StoredProcedure [dbo].[SP_Register]    Script Date: 09.08.2015 23:38:38 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE PROC [dbo].[SP_Register] @Login nvarchar(30), @Email varchar(30), @Password char(64), @ImageName nvarchar(16)
AS
BEGIN
	DECLARE @TempLogin nvarchar(30);
	--Login existence check
	IF EXISTS(SELECT [Login] FROM [dbo].[User] WHERE [Login] = @Login) 
		RETURN 0;

	INSERT INTO [dbo].[User] ([Login], [Email], [Password], [RegistrationDate], [ImageName])
	VALUES(@Login, @Email, @Password, GETDATE(), @ImageName)
	
	RETURN 1;
END

GO

