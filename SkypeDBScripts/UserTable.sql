USE [D:\STEP ACEDAMY\C#\MYWPFPROJECTSDBOPRODUCT\SERVER\DAO\SKYPEDATABASE.MDF]
GO

/****** Object:  Table [dbo].[User]    Script Date: 09.08.2015 23:36:06 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[User](
	[UserID] [int] IDENTITY(1,1) NOT NULL,
	[Login] [nvarchar](30) NOT NULL,
	[Email] [varchar](30) NOT NULL,
	[Password] [char](64) NOT NULL,
	[RegistrationDate] [date] NOT NULL,
	[ImageName] [nvarchar](16) NULL,
 CONSTRAINT [PK_User] PRIMARY KEY CLUSTERED 
(
	[UserID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING ON
GO
