USE [master]
GO
/****** Object:  Database [aoai-proxy]    Script Date: 14/12/2023 10:18:33 PM ******/
CREATE DATABASE [aoai-proxy]
GO
ALTER DATABASE [aoai-proxy] SET COMPATIBILITY_LEVEL = 160
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
    EXEC [aoai-proxy].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [aoai-proxy] SET ANSI_NULL_DEFAULT OFF
GO
ALTER DATABASE [aoai-proxy] SET ANSI_NULLS OFF
GO
ALTER DATABASE [aoai-proxy] SET ANSI_PADDING OFF
GO
ALTER DATABASE [aoai-proxy] SET ANSI_WARNINGS OFF
GO
ALTER DATABASE [aoai-proxy] SET ARITHABORT OFF
GO
ALTER DATABASE [aoai-proxy] SET AUTO_CLOSE OFF
GO
ALTER DATABASE [aoai-proxy] SET AUTO_SHRINK OFF
GO
ALTER DATABASE [aoai-proxy] SET AUTO_UPDATE_STATISTICS ON
GO
ALTER DATABASE [aoai-proxy] SET CURSOR_CLOSE_ON_COMMIT OFF
GO
ALTER DATABASE [aoai-proxy] SET CURSOR_DEFAULT  GLOBAL
GO
ALTER DATABASE [aoai-proxy] SET CONCAT_NULL_YIELDS_NULL OFF
GO
ALTER DATABASE [aoai-proxy] SET NUMERIC_ROUNDABORT OFF
GO
ALTER DATABASE [aoai-proxy] SET QUOTED_IDENTIFIER OFF
GO
ALTER DATABASE [aoai-proxy] SET RECURSIVE_TRIGGERS OFF
GO
ALTER DATABASE [aoai-proxy] SET  DISABLE_BROKER
GO
ALTER DATABASE [aoai-proxy] SET AUTO_UPDATE_STATISTICS_ASYNC OFF
GO
ALTER DATABASE [aoai-proxy] SET DATE_CORRELATION_OPTIMIZATION OFF
GO
ALTER DATABASE [aoai-proxy] SET TRUSTWORTHY OFF
GO
ALTER DATABASE [aoai-proxy] SET ALLOW_SNAPSHOT_ISOLATION OFF
GO
ALTER DATABASE [aoai-proxy] SET PARAMETERIZATION SIMPLE
GO
ALTER DATABASE [aoai-proxy] SET READ_COMMITTED_SNAPSHOT OFF
GO
ALTER DATABASE [aoai-proxy] SET HONOR_BROKER_PRIORITY OFF
GO
ALTER DATABASE [aoai-proxy] SET RECOVERY SIMPLE
GO
ALTER DATABASE [aoai-proxy] SET  MULTI_USER
GO
ALTER DATABASE [aoai-proxy] SET PAGE_VERIFY CHECKSUM
GO
ALTER DATABASE [aoai-proxy] SET DB_CHAINING OFF
GO
ALTER DATABASE [aoai-proxy] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF )
GO
ALTER DATABASE [aoai-proxy] SET TARGET_RECOVERY_TIME = 60 SECONDS
GO
ALTER DATABASE [aoai-proxy] SET DELAYED_DURABILITY = DISABLED
GO
ALTER DATABASE [aoai-proxy] SET ACCELERATED_DATABASE_RECOVERY = OFF
GO
ALTER DATABASE [aoai-proxy] SET QUERY_STORE = ON
GO
ALTER DATABASE [aoai-proxy] SET QUERY_STORE (OPERATION_MODE = READ_WRITE, CLEANUP_POLICY = (STALE_QUERY_THRESHOLD_DAYS = 30), DATA_FLUSH_INTERVAL_SECONDS = 900, INTERVAL_LENGTH_MINUTES = 60, MAX_STORAGE_SIZE_MB = 1000, QUERY_CAPTURE_MODE = AUTO, SIZE_BASED_CLEANUP_MODE = AUTO, MAX_PLANS_PER_QUERY = 200, WAIT_STATS_CAPTURE_MODE = ON)
GO
USE [aoai-proxy]
GO
/****** Object:  Table [dbo].[Event]    Script Date: 14/12/2023 10:18:33 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Event]
(
    [EventID] [nvarchar](50) NOT NULL,
    [OwnerID] [uniqueidentifier] NOT NULL,
    [EventName] [nvarchar](64) NOT NULL,
    [EventMarkdown] [nvarchar](4000) NOT NULL,
    [StartUtc] [datetime2](7) NOT NULL,
    [EndUtc] [datetime2](7) NOT NULL,
    [OrganizerName] [nvarchar](128) NOT NULL,
    [OrganizerEmail] [nvarchar](128) NOT NULL,
    [EventUrl] [nvarchar](256) NOT NULL,
    [EventUrlText] [nvarchar](256) NOT NULL,
    [MaxTokenCap] [int] NOT NULL,
    [SingleCode] [bit] NOT NULL,
    [Active] [bit] NOT NULL,
    [DailyRequestCap] [int] NOT NULL,
    CONSTRAINT [PK_Event] PRIMARY KEY CLUSTERED
(
	[EventID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EventAttendee]    Script Date: 14/12/2023 10:18:33 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EventAttendee]
(
    [EntraID] [nvarchar](128) NOT NULL,
    [EventID] [nvarchar](50) NOT NULL,
    [Active] [bit] NOT NULL,
    [TotalRequests] [int] NOT NULL,
    [ApiKey] [uniqueidentifier] NOT NULL,
    CONSTRAINT [PK_User] PRIMARY KEY CLUSTERED
(
	[EntraID] ASC,
	[EventID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EventCatalog]    Script Date: 14/12/2023 10:18:33 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EventCatalog]
(
    [EventID] [nvarchar](50) NOT NULL,
    [CatalogID] [uniqueidentifier] NOT NULL,
    CONSTRAINT [PK_EventModel_1] PRIMARY KEY CLUSTERED
(
	[EventID] ASC,
	[CatalogID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EventManager]    Script Date: 14/12/2023 10:18:33 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EventManager]
(
    [OwnerID] [uniqueidentifier] NOT NULL,
    [EventID] [nvarchar](50) NOT NULL,
    [Creator] [bit] NOT NULL,
    CONSTRAINT [PK_EventManagers] PRIMARY KEY CLUSTERED
(
	[OwnerID] ASC,
	[EventID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EventOwner]    Script Date: 14/12/2023 10:18:33 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EventOwner]
(
    [EntraID] [nvarchar](128) NOT NULL,
    [OwnerID] [uniqueidentifier] ROWGUIDCOL NOT NULL,
    CONSTRAINT [PK_Group] PRIMARY KEY CLUSTERED
(
	[OwnerID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[OwnerCatalog]    Script Date: 14/12/2023 10:18:33 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OwnerCatalog]
(
    [OwnerID] [uniqueidentifier] NOT NULL,
    [CatalogID] [uniqueidentifier] ROWGUIDCOL NOT NULL,
    [FriendlyName] [nvarchar](64) NOT NULL,
    [DeploymentName] [nvarchar](64) NOT NULL,
    [ResourceName] [nvarchar](64) NOT NULL,
    [EndpointKey] [nvarchar](128) NOT NULL,
    [ModelClass] [nvarchar](64) NOT NULL,
    [Active] [bit] NOT NULL,
    CONSTRAINT [PK_GroupModel] PRIMARY KEY CLUSTERED
(
	[CatalogID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Index [IX_Event]    Script Date: 14/12/2023 10:18:33 PM ******/
CREATE NONCLUSTERED INDEX [IX_Event] ON [dbo].[Event]
(
	[OwnerID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_EventModels]    Script Date: 14/12/2023 10:18:33 PM ******/
CREATE NONCLUSTERED INDEX [IX_EventModels] ON [dbo].[EventCatalog]
(
	[EventID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_GroupModels_1]    Script Date: 14/12/2023 10:18:33 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_GroupModels_1] ON [dbo].[OwnerCatalog]
(
	[OwnerID] ASC,
	[FriendlyName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Event] ADD  CONSTRAINT [DF_Event_EventID]  DEFAULT (newid()) FOR [EventID]
GO
ALTER TABLE [dbo].[Event] ADD  CONSTRAINT [DF_Event_GroupID]  DEFAULT (newid()) FOR [OwnerID]
GO
ALTER TABLE [dbo].[EventOwner] ADD  CONSTRAINT [DF_Group_GroupID]  DEFAULT (newid()) FOR [OwnerID]
GO
ALTER TABLE [dbo].[OwnerCatalog] ADD  CONSTRAINT [DF_GroupModels_CatalogID]  DEFAULT (newid()) FOR [CatalogID]
GO
ALTER TABLE [dbo].[EventAttendee]  WITH CHECK ADD  CONSTRAINT [FK_User_Event] FOREIGN KEY([EventID])
REFERENCES [dbo].[Event] ([EventID])
GO
ALTER TABLE [dbo].[EventAttendee] CHECK CONSTRAINT [FK_User_Event]
GO
ALTER TABLE [dbo].[EventCatalog]  WITH CHECK ADD  CONSTRAINT [FK_EventModel_Event] FOREIGN KEY([EventID])
REFERENCES [dbo].[Event] ([EventID])
GO
ALTER TABLE [dbo].[EventCatalog] CHECK CONSTRAINT [FK_EventModel_Event]
GO
ALTER TABLE [dbo].[EventCatalog]  WITH CHECK ADD  CONSTRAINT [FK_EventModel_GroupModel] FOREIGN KEY([CatalogID])
REFERENCES [dbo].[OwnerCatalog] ([CatalogID])
GO
ALTER TABLE [dbo].[EventCatalog] CHECK CONSTRAINT [FK_EventModel_GroupModel]
GO
ALTER TABLE [dbo].[EventManager]  WITH CHECK ADD  CONSTRAINT [FK_EventManager_Event] FOREIGN KEY([EventID])
REFERENCES [dbo].[Event] ([EventID])
GO
ALTER TABLE [dbo].[EventManager] CHECK CONSTRAINT [FK_EventManager_Event]
GO
ALTER TABLE [dbo].[EventManager]  WITH CHECK ADD  CONSTRAINT [FK_EventManagers_EventOwner] FOREIGN KEY([OwnerID])
REFERENCES [dbo].[EventOwner] ([OwnerID])
GO
ALTER TABLE [dbo].[EventManager] CHECK CONSTRAINT [FK_EventManagers_EventOwner]
GO
ALTER TABLE [dbo].[OwnerCatalog]  WITH CHECK ADD  CONSTRAINT [FK_GroupModels_Group] FOREIGN KEY([OwnerID])
REFERENCES [dbo].[EventOwner] ([OwnerID])
GO
ALTER TABLE [dbo].[OwnerCatalog] CHECK CONSTRAINT [FK_GroupModels_Group]
GO
/****** Object:  StoredProcedure [dbo].[EventAdd]    Script Date: 14/12/2023 10:18:33 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[EventAdd]
    @OwnerID uniqueidentifier,
    @EventName NVARCHAR(64),
    @EventMarkdown NVARCHAR(4000),
    @StartUtc DATETIME2,
    @EndUtc DATETIME2,
    @OrganizerName NVARCHAR(128),
    @OrganizerEmail NVARCHAR(128),
    @EventUrl NVARCHAR(256),
    @EventUrlText NVARCHAR(256),
    @MaxTokenCap INT,
    @SingleCode Bit,
    @DailyRequestCap INT
AS
BEGIN

    SET NOCOUNT ON;

    DECLARE @Hash VARBINARY(64);

    DECLARE @Guid1 UNIQUEIDENTIFIER = NEWID();
    DECLARE @Guid2 UNIQUEIDENTIFIER = NEWID();
    DECLARE @GuidString NVARCHAR(128);

    SET @GuidString = CAST(@Guid1 AS NVARCHAR(36)) + CAST(@Guid2 AS NVARCHAR(36));

    SET @Hash = HASHBYTES('SHA2_256', @GuidString);

    DECLARE @HashString NVARCHAR(64);
    SET @HashString = CONVERT(NVARCHAR(64), @Hash, 2);

    DECLARE @Half1 NVARCHAR(4), @Half2 NVARCHAR(4);
    SET @Half1 = SUBSTRING(@HashString, 1, 4);
    SET @Half2 = SUBSTRING(@HashString, 5, 4);

    DECLARE @FinalHash NVARCHAR(11);
    SET @FinalHash = CONCAT(@Half1, '-', @Half2);


    INSERT INTO Event
        (
        EventID,
        OwnerID,
        EventName,
        EventMarkdown,
        StartUtc,
        EndUtc,
        OrganizerName,
        OrganizerEmail,
        EventUrl,
        EventUrlText,
        MaxTokenCap,
        SingleCode,
        Active,
        DailyRequestCap
        )
    VALUES
        (
            @FinalHash,
            @OwnerID,
            @EventName,
            @EventMarkdown,
            @StartUtc,
            @EndUtc,
            @OrganizerName,
            @OrganizerEmail,
            @EventUrl,
            @EventUrlText,
            @MaxTokenCap,
            @SingleCode,
            1,
            @DailyRequestCap
    );


    INSERT INTO EventManager
        (
        OwnerID,
        EventID,
        Creator
        )
    VALUES
        (
            @OwnerID,
            @FinalHash,
            1
	)

    SELECT *
    FROM Event
    WHERE EventID = @FinalHash

END;
GO
/****** Object:  StoredProcedure [dbo].[EventAttendeeAdd]    Script Date: 14/12/2023 10:18:33 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[EventAttendeeAdd]
    @EventID varchar(16),
    @EntraID nvarchar(128)

AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @GroupID uniqueidentifier;
    DECLARE @ApiKey uniqueidentifier;

    -- Generate a new ApiKey
    SET @ApiKey = NEWID();

    -- Insert a new row into the EventUser table
    INSERT INTO EventAttendee
        (EntraID, EventID, Active, TotalRequests, ApiKey)
    VALUES
        (@EntraID, @EventID, 1, 0, @ApiKey);
END

GO
/****** Object:  StoredProcedure [dbo].[EventAttendeeAuthorized]    Script Date: 14/12/2023 10:18:33 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[EventAttendeeAuthorized]
    @EntraID nvarchar(128),
    @EventID nvarchar(50)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @AttendeeActive bit;
    DECLARE @EventActive bit;
    DECLARE @StartUtc datetime2(7);
    DECLARE @EndUtc datetime2(7);
    DECLARE @CurrentUtc datetime2(7) = GETUTCDATE();
    DECLARE @TotalRequests int;
    DECLARE @DailyRequestCap int;

    SELECT @AttendeeActive = Active, @TotalRequests = TotalRequests
    FROM EventAttendee
    WHERE EntraID = @EntraID AND EventID = @EventID;
    SELECT @StartUtc = StartUtc, @EndUtc = EndUtc, @DailyRequestCap = DailyRequestCap, @EventActive = Active
    FROM Event
    WHERE EventID = @EventID;

    IF @AttendeeActive = 1 AND @EventActive = 1 AND @CurrentUtc BETWEEN @StartUtc AND @EndUtc AND @TotalRequests < @DailyRequestCap
    BEGIN
        SELECT 1 AS IsAuthorized;
    END
    ELSE
    BEGIN
        SELECT 0 AS IsAuthorized;
    END
END;
GO
/****** Object:  StoredProcedure [dbo].[EventAttendeeDeleteAll]    Script Date: 14/12/2023 10:18:33 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[EventAttendeeDeleteAll]
    @EventID varchar(16)
AS
BEGIN
    SET NOCOUNT ON;

    -- Delete rows from the EventUser table
    DELETE FROM EventAttendee
    WHERE EventID = @EventID;
END

GO
/****** Object:  StoredProcedure [dbo].[EventCatalogAdd]    Script Date: 14/12/2023 10:18:33 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[EventCatalogAdd]
    @EventID varchar(16),
    @CatalogID uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;

    -- Insert a new row into the EventModel table
    INSERT INTO EventCatalog
        (EventID, CatalogID)
    VALUES
        (@EventID, @CatalogID);
END
GO
/****** Object:  StoredProcedure [dbo].[EventCatalogDelete]    Script Date: 14/12/2023 10:18:33 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[EventCatalogDelete]
    @CatalogID uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;

    -- Delete row from the EventModel table
    DELETE FROM EventCatalog
    WHERE CatalogID = @CatalogID;
END

GO
/****** Object:  StoredProcedure [dbo].[EventCatalogGetByEvent]    Script Date: 14/12/2023 10:18:33 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[EventCatalogGetByEvent]
    @EventID nvarchar(50)
AS
BEGIN
    SELECT
        OC.FriendlyName,
        OC.DeploymentName,
        OC.ResourceName,
        OC.EndpointKey,
        OC.ModelClass
    FROM
        EventCatalog EC
        INNER JOIN
        OwnerCatalog OC ON EC.CatalogID = OC.CatalogID
    WHERE
        EC.EventID = @EventID AND
        OC.Active = 1;
END;
GO
/****** Object:  StoredProcedure [dbo].[EventCatalogList]    Script Date: 14/12/2023 10:18:33 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[EventCatalogList]
    @EventID varchar(16)
AS
BEGIN
    SET NOCOUNT ON;

    -- Select rows from the EventModel table where EventID matches the input parameter
    SELECT *
    FROM EventCatalog
    WHERE EventID = @EventID;
END

GO
/****** Object:  StoredProcedure [dbo].[EventDelete]    Script Date: 14/12/2023 10:18:33 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[EventDelete]
    @EventID NVARCHAR(16)
AS
BEGIN
    DELETE FROM Event
    WHERE EventID = @EventID;

    DELETE FROM EventManager
	WHERE EventID = @EventID
END;
GO
/****** Object:  StoredProcedure [dbo].[EventGetByEventID]    Script Date: 14/12/2023 10:18:33 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[EventGetByEventID]
    @EventID NVARCHAR(16)
AS
BEGIN
    SELECT *
    FROM Event
    WHERE EventID = @EventID;

END;
GO
/****** Object:  StoredProcedure [dbo].[EventListByOwner]    Script Date: 14/12/2023 10:18:33 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[EventListByOwner]
    @OwnerID uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;

    -- Select specific columns from the table where GroupID matches the input parameter

    SELECT E.EventID, E.OwnerID, E.EventName, E.EventMarkdown, E.StartUtc, E.EndUtc, E.OrganizerName, E.OrganizerEmail, E.EventUrl, E.EventUrlText, E.MaxTokenCap, E.SingleCode, E.Active
    FROM EventOwner EO
        JOIN EventManager EM ON EO.OwnerID = EM.OwnerID
        JOIN Event E ON EM.EventID = E.EventID
    WHERE EO.OwnerID = @OwnerID

END

GO
/****** Object:  StoredProcedure [dbo].[EventOwnerAdd]    Script Date: 14/12/2023 10:18:33 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
Create PROCEDURE [dbo].[EventOwnerAdd]
    @OwnerID uniqueidentifier,
    @EventID NVARCHAR(50)
AS
BEGIN

    SET NOCOUNT ON;

    INSERT INTO EventManager
        (
        OwnerID,
        EventID,
        Creator
        )
    VALUES
        (
            @OwnerID,
            @EventID,
            0
	)

END;
GO
/****** Object:  StoredProcedure [dbo].[EventOwnerListAll]    Script Date: 14/12/2023 10:18:33 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[EventOwnerListAll]
AS
BEGIN
    SET NOCOUNT ON;

    -- Select all rows from the GroupAdmin table
    SELECT *
    FROM EventOwner;
END

GO
/****** Object:  StoredProcedure [dbo].[EventUpdate]    Script Date: 14/12/2023 10:18:33 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[EventUpdate]
    @EventID NVARCHAR(16),
    @OwnerID uniqueidentifier,
    @EventName NVARCHAR(64),
    @EventMarkdown NVARCHAR(4000),
    @StartUtc DATETIME2,
    @EndUtc DATETIME2,
    @OrganizerName NVARCHAR(128),
    @OrganizerEmail NVARCHAR(128),
    @EventUrl NVARCHAR(256),
    @EventUrlText NVARCHAR(256),
    @MaxTokenCap INT,
    @SingleCode Bit,
    @Active BIT
AS
BEGIN
    UPDATE Event
    SET
        OwnerID = @OwnerID,
        EventName = @EventName,
        EventMarkdown = @EventMarkdown,
        StartUtc = @StartUtc,
        EndUtc = @EndUtc,
        OrganizerName = @OrganizerName,
        OrganizerEmail = @OrganizerEmail,
        EventUrl = @EventUrl,
        EventUrlText = @EventUrlText,
        MaxTokenCap = @MaxTokenCap,
        SingleCode = @SingleCode,
        Active = @Active
    WHERE
        EventID = @EventID;
END;
GO
/****** Object:  StoredProcedure [dbo].[OwnerGet]    Script Date: 14/12/2023 10:18:33 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[OwnerGet]
    @EntraID VARCHAR(128)
AS
BEGIN

    SET NOCOUNT ON;

    DECLARE @OwnerID UNIQUEIDENTIFIER

    SELECT @OwnerID = OwnerID
    FROM [EventOwner]
    WHERE EntraID = @EntraID

    IF @OwnerID IS NULL
    BEGIN
        SET @OwnerID = NEWID()

        INSERT INTO [EventOwner]
            (EntraID, OwnerID)
        VALUES
            (@EntraID, @OwnerID)

    END

    SELECT @OwnerID as OwnerID

END
GO
/****** Object:  StoredProcedure [dbo].[OwnerModelAdd]    Script Date: 14/12/2023 10:18:33 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[OwnerModelAdd]
    @OwnerID uniqueidentifier,
    @FriendlyName nvarchar(64),
    @DeploymentName nvarchar(64),
    @ResourceName nvarchar(64),
    @EndpointKey nvarchar(128),
    @ModelClass nvarchar(64),
    @Active bit
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CatalogID uniqueidentifier;

    -- Generate a new CatalogID
    SET @CatalogID = NEWID();

    -- Insert a new row into the table
    INSERT INTO OwnerModel
        (OwnerID, CatalogID, FriendlyName, DeploymentName, ResourceName, EndpointKey, ModelClass, Active)
    VALUES
        (@OwnerID, @CatalogID, @FriendlyName, @DeploymentName, @ResourceName, @EndpointKey, @ModelClass, @Active);
END

GO
/****** Object:  StoredProcedure [dbo].[OwnerModelDelete]    Script Date: 14/12/2023 10:18:33 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[OwnerModelDelete]
    @CatalogID uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;

    -- Delete row from the GroupModel table
    DELETE FROM OwnerModel
    WHERE CatalogID = @CatalogID;
END

GO
/****** Object:  StoredProcedure [dbo].[OwnerModelList]    Script Date: 14/12/2023 10:18:33 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[OwnerModelList]
    @OwnerID uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;

    -- Select rows from the GroupModel table where GroupID matches the input parameter
    SELECT *
    FROM OwnerModel
    WHERE OwnerID = @OwnerID;
END

GO
/****** Object:  StoredProcedure [dbo].[OwnerModelUpdate]    Script Date: 14/12/2023 10:18:33 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[OwnerModelUpdate]
    @CatalogID uniqueidentifier,
    @FriendlyName nvarchar(64),
    @DeploymentName nvarchar(64),
    @ResourceName nvarchar(64),
    @EndpointKey nvarchar(128),
    @ModelClass nvarchar(64),
    @Active bit
AS
BEGIN
    SET NOCOUNT ON;

    -- Update the row in the table
    UPDATE OwnerModel
    SET FriendlyName = @FriendlyName,
        DeploymentName = @DeploymentName,
        ResourceName = @ResourceName,
        EndpointKey = @EndpointKey,
        ModelClass = @ModelClass,
        Active = @Active
    WHERE CatalogID = @CatalogID;
END

GO
/****** Object:  StoredProcedure [dbo].[SystemInit]    Script Date: 14/12/2023 10:18:33 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[SystemInit]
AS
BEGIN
    -- SET NOCOUNT ON added to prevent extra result sets from
    -- interfering with SELECT statements.
    SET NOCOUNT ON;

    DELETE FROM EventAttendee
    DELETE FROM [Event]
    DELETE FROM EventManager
    DELETE FROM EventCatalog
    DELETE FROM OwnerCatalog
    DELETE FROM EventOwner
END
GO
USE [master]
GO
ALTER DATABASE [aoai-proxy] SET  READ_WRITE
GO
