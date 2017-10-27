CREATE TABLE [dbo].[Messages] (
    [Id]            UNIQUEIDENTIFIER NOT NULL,
    [GroupId]       UNIQUEIDENTIFIER NOT NULL,
    [ParentId]      UNIQUEIDENTIFIER NULL,
    [CreatedById]   UNIQUEIDENTIFIER NOT NULL,
    [TypeId]        UNIQUEIDENTIFIER NOT NULL,
    [Text]          NVARCHAR (MAX)   NULL,
    [AttachmentUrl] NVARCHAR (MAX)   NULL,
    [CreatedOn]     DATETIME         NOT NULL,
    [UpdatedOn]     DATETIME         NULL
);


