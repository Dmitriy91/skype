USE [D:\STEP ACEDAMY\C#\MYWPFPROJECTSDBOPRODUCT\SERVER\DAO\SKYPEDATABASE.MDF]
GO

/****** Object:  StoredProcedure [dbo].[SP_Contact_SEL_byLogin]    Script Date: 09.08.2015 23:37:25 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[SP_Contact_SEL_byLogin]
	@Login nvarchar(30)
AS
BEGIN TRY
	SELECT UserID, [Login], [Email], [ImageName] 
	FROM [User]
	WHERE [Login] LIKE @Login + '%'
	RETURN 1;
END TRY
BEGIN CATCH
	RETURN 0;
END CATCH


GO

