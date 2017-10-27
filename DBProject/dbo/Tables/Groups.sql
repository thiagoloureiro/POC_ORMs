CREATE TABLE [dbo].[Groups] (
    [Id]         UNIQUEIDENTIFIER NOT NULL,
    [Name]       NVARCHAR (MAX)   NULL,
    [IsPublic]   BIT              NOT NULL,
    [AvatarUrl]  NVARCHAR (MAX)   NULL,
    [TypeId]     UNIQUEIDENTIFIER NOT NULL,
    [CreatedOn]  DATETIME         NOT NULL,
    [UpdatedOn]  DATETIME         NULL,
    [ArchivedOn] DATETIME         NULL,
    CONSTRAINT [PK_dbo.Groups] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_TypeId]
    ON [dbo].[Groups]([TypeId] ASC);

