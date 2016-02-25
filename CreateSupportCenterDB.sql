USE [master]
GO

/*
***********************************************************************
*******************        Create Database       **********************
***********************************************************************
*/
--Drop if allready exists
IF EXISTS (SELECT name FROM master.sys.databases WHERE name = N'SupportCenterDB')
	DROP DATABASE [SupportCenterDB]
GO

CREATE DATABASE [SupportCenterDB]
 GO

USE [SupportCenterDB]
GO

/*
***********************************************************************
********************        Create Tables       ***********************
***********************************************************************
*/
--Table [dbo].[Ticket]
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Ticket](
	[TicketNumber] [int] IDENTITY(1,1) NOT NULL,
	[AccountId] [int] NOT NULL,
	[Text] [nvarchar](255) NOT NULL,
	[DateOpened] [datetime] NOT NULL,
	[State] [tinyint] NOT NULL,
	[DeviceName] [nvarchar](255),
 CONSTRAINT [Ticket_PK] PRIMARY KEY NONCLUSTERED 
(
	[TicketNumber] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

--Table [dbo].[TicketResponse]
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[TicketResponse](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Ticket_TicketNumber] [int] NOT NULL,
	[Text] [nvarchar](255) NOT NULL,
	[Date] [datetime] NOT NULL,
	[IsClientResponse] [bit] NOT NULL,
 CONSTRAINT [TicketResponse_PK] PRIMARY KEY NONCLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[TicketResponse] ADD  CONSTRAINT [DF__Ticket__IsClientResponse]  DEFAULT ((0)) FOR [IsClientResponse]
GO

ALTER TABLE [dbo].[TicketResponse]  WITH CHECK ADD  CONSTRAINT [TicketResponse_FK_Ticket] FOREIGN KEY([Ticket_TicketNumber])
REFERENCES [dbo].[Ticket] ([TicketNumber])
GO

ALTER TABLE [dbo].[TicketResponse] CHECK CONSTRAINT [TicketResponse_FK_Ticket]
GO


/*
***********************************************************************
******************        Insert Dummy Data       *********************
***********************************************************************
*/
DECLARE @ticket1 INT
DECLARE @ticket2 INT
DECLARE @ticket3 INT

INSERT INTO [dbo].[Ticket]([AccountId], [Text], [DateOpened], [State])
     VALUES (1, 'Cannot login on webmail', '2012-09-09 13:05:59.000' , 4)
SELECT @ticket1= SCOPE_IDENTITY()

INSERT INTO [dbo].[TicketResponse]([Ticket_TicketNumber], [Text], [Date], [IsClientResponse])
     VALUES (@ticket1, 'Account was locked', '2012-09-09 13:24:48.000', 0)
			, (@ticket1, 'Account is unlocked, and pwd is reset', '2012-09-09 13:29:11.000', 0)
			, (@ticket1, 'Login ok, pwd is changed', '2012-09-10 07:22:36.000', 1)

INSERT INTO [dbo].[Ticket]([AccountId], [Text], [DateOpened], [State])
	 VALUES (2, 'Have no internet access', '2012-11-05 09:45:13.000' , 2)
SELECT @ticket2= SCOPE_IDENTITY()

INSERT INTO [dbo].[TicketResponse]([Ticket_TicketNumber], [Text], [Date], [IsClientResponse])
     VALUES (@ticket2, 'Check if cable is plugged in correctly', '2012-11-05 11:25:42.000', 0)

INSERT INTO [dbo].[Ticket]([AccountId], [Text], [DateOpened], [State], [DeviceName])
	  VALUES (3, 'Blue screen!', '2012-12-14 19:15:32.000' , 1, 'PC-123456')
SELECT @ticket3= SCOPE_IDENTITY()


/*
***********************************************************************
*********************        Create SP       **************************
***********************************************************************
*/
GO

CREATE PROCEDURE sp_CloseTicket
	@ticketNumber INT
 AS
 BEGIN
	UPDATE Ticket SET State = 4
		WHERE TicketNumber = @ticketNumber
 END

 GO