USE [ImageGalleryDB]
GO

/****** Object: Table [dbo].[Images] Script Date: 12/20/2017 5:12:52 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Image] (
    [Id]       UNIQUEIDENTIFIER NOT NULL,
    [OwnerId]  NVARCHAR (50)    NOT NULL,
    [Title]    NVARCHAR (150)   NOT NULL,
    [FileName] NVARCHAR (200)   NOT NULL
);


