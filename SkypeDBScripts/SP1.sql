USE [D:\STEP ACEDAMY\C#\MYWPFPROJECTSDBOPRODUCT\SERVER\DAO\SKYPEDATABASE.MDF]
GO

/****** Object:  StoredProcedure [dbo].[SP_Contact_INS]    Script Date: 09.08.2015 23:37:18 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[SP_Contact_INS]
	@UserID int,
	@ContactID int
AS
BEGIN TRY
	IF(@UserID = @ContactID)
		RETURN 2

	IF NOT EXISTS (SELECT u.UserID, c.ContactID FROM [dbo].[User] AS u
	JOIN [dbo].[Contact] as c ON c.UserID = u.UserID
	WHERE u.UserID = @UserID AND c.ContactID = @ContactID) 
	BEGIN
		INSERT [dbo].[Contact]
		(
			[UserID],
			[ContactID]
		)
		VALUES
		(
			@UserID,
			@ContactID
		)
		RETURN 1;
	END
	RETURN 2;
END TRY
BEGIN CATCH
	RETURN 0;
END CATCH

GO

