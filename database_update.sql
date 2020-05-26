USE [UPM]
GO

--------------------------------- Приход по мешкам ----------------------------

/****** Object:  Table [dbo].[CorrectionsAtShiftEnd]    Script Date: 14.05.2020 6:55:02 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

DROP TABLE CorrectionsAtShiftEnd
go

CREATE TABLE [dbo].[CorrectionsAtShiftEnd](
	[CorrectionID] [int] IDENTITY(0,1) NOT NULL,
	[DateShift] [datetime] NULL,
	[IsNight] [bit] NULL,
	[Material] [nvarchar](20) NULL,
	[BagWeight] [int] NULL,
	[Income] [int] NULL,
	[BagQuantity] [int] NULL,
	[CorrectionValue] [int] NULL,
	[CorrectionText] [nvarchar](256) NULL,
PRIMARY KEY CLUSTERED 
(
	[CorrectionID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

------------------------------------------------------ Транспортые заказы ----------------------------

CREATE TABLE [dbo].[LUPLastBag](
	[MaterialNumber] [varchar](10) NULL,
	[LastBagTime] [datetime] NULL,
	[LastTransferOrder] [bigint] NOT NULL
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[LUPLastBag] ADD  DEFAULT ((9999999999.)) FOR [LastTransferOrder]
GO


CREATE PROCEDURE [dbo].[SetLastBag]
	(
	@MaterialNumber varchar(10),		-- Материал
	@LastBagTime [DateTime]   -- Время последнего загруженного мешка,
	,@LastTransferOrder bigint = 999999999
	)
AS
	DECLARE @count smallint

	SET @count = ISNULL(
		(SELECT COUNT(*) 
		FROM LUPLastBag
		WHERE MaterialNumber=@MaterialNumber), 0)

	IF @count>0
	BEGIN	
	UPDATE LUPLastBag 
		SET LastBagTime = @LastBagTime
		--,LastTransferOrder=@LastTransferOrder

		WHERE MaterialNumber=@MaterialNumber;
	END	

	ELSE
	BEGIN	
--	INSERT INTO LUPLastBag VALUES(@MaterialNumber, @LastBagTime, @LastTransferOrder);
	INSERT INTO LUPLastBag(MaterialNumber,LastBagTime,LastTransferOrder) VALUES(@MaterialNumber, @LastBagTime,@LastTransferOrder);
	END
	RETURN
GO


CREATE PROCEDURE [dbo].[GetLastBag]
	(
	@MaterialNumber varchar(10)		-- Материал
	)
AS
	SELECT LastBagTime
	, LastTransferOrder 
	FROM LUPLastBag
		WHERE MaterialNumber=@MaterialNumber

GO