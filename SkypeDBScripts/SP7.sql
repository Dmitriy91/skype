USE [D:\STEP ACEDAMY\C#\MYWPFPROJECTSDBOPRODUCT\SERVER\DAO\SKYPEDATABASE.MDF]
GO

/****** Object:  StoredProcedure [dbo].[SP_ContactPair_DEL]    Script Date: 09.08.2015 23:38:24 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[SP_ContactPair_DEL]
	@UserID int,
	@ContactID int
AS
BEGIN TRY
	IF(@UserID = @ContactID)
		RETURN 0

	IF EXISTS (SELECT UserID, ContactID FROM [dbo].[Contact] 
		WHERE UserID = @UserID AND ContactID = @ContactID)
		AND
		EXISTS (SELECT ContactID, UserID FROM [dbo].[Contact] 
		WHERE UserID = @UserID AND ContactID = @ContactID) 
	BEGIN
		DELETE FROM [dbo].[Contact]
		WHERE (ContactID = @ContactID AND UserID = @UserID)
		OR (ContactID = @UserID AND UserID = @ContactID)
		RETURN 1;
	END

	RETURN 0;
END TRY
BEGIN CATCH
	RETURN 0;
END CATCH
GO

