USE [D:\STEP ACEDAMY\C#\MYWPFPROJECTSDBOPRODUCT\SERVER\DAO\SKYPEDATABASE.MDF]
GO

/****** Object:  StoredProcedure [dbo].[SP_User_INS]    Script Date: 09.08.2015 23:38:54 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[SP_User_INS]
	@UserID int OUTPUT,
	@Login nvarchar(30), 
	@Email nvarchar(30), 
	@Password nvarchar(64),
	@RegistrationDate date,
	@ImageName nvarchar(16)
AS
BEGIN TRY
	IF NOT EXISTS (SELECT * FROM [User] WHERE [Login] = @Login)
	BEGIN
		INSERT [User]
		(
			[Login],
			Email,
			[Password],
			RegistrationDate,
			ImageName
		)
		VALUES
		(
			@Login,
			@Email,
			@Password,
			@RegistrationDate,
			@ImageName
		)

		SET @UserID = @@IDENTITY
		RETURN 1;
	END
	RETURN 2;
END TRY
BEGIN CATCH
	RETURN 0;
END CATCH

GO

