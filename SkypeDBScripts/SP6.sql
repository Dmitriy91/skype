USE [D:\STEP ACEDAMY\C#\MYWPFPROJECTSDBOPRODUCT\SERVER\DAO\SKYPEDATABASE.MDF]
GO

/****** Object:  StoredProcedure [dbo].[SP_ContactList_SEL_byUserID]    Script Date: 09.08.2015 23:37:59 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[SP_ContactList_SEL_byUserID]
	@UserID int
AS
BEGIN TRY
	SELECT u.UserID, u.Email, u.[Login], u.[ImageName] 
	FROM Contact AS c
	JOIN [User] AS u ON u.UserID = c.ContactID
	WHERE c.UserID = @UserID
	RETURN 1;
END TRY
BEGIN CATCH
	RETURN 0;
END CATCH


GO

