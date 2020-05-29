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


------------------------------------------


CREATE TABLE [dbo].[MaterialDataAtShiftEnd](
	[MaterialDataAtShiftEndID] [int] IDENTITY(1,1) NOT NULL,
	[DateShift] [date] NULL,
	[IsNight] [bit] NULL,
	[Material] [nvarchar](20) NULL,
	[BagWeight] [int] NULL,
	[BagQuantity_Start] [int] NULL,
	[Income] [int] NULL,
	[Loaded] [int] NULL,
	[BagQuantity_End] [int] NULL,
	[CorrectionValue] [int] NULL,
	[CorrectionText] [nvarchar](256) NULL
)
GO

CREATE PROCEDURE SetCorrectionsAtShiftEnd(
	@DateShift [date],
	@IsNight [bit] ,
	@Material [nvarchar](20),
	@BagWeight [int] ,
	@Income [int] ,
	@BagQuantity [int] ,
	@CorrectionValue [int] ,
	@CorrectionText [nvarchar](256) 
)
AS
insert into CorrectionsAtShiftEnd(DateShift,IsNight,Material,BagWeight,Income,BagQuantity,CorrectionValue,CorrectionText) 
VALUES(@DateShift,@IsNight,@Material,@BagWeight,@Income,@BagQuantity,@CorrectionValue,@CorrectionText);

DECLARE
	@BagQuantity_AtEnd [int]=@BagQuantity

/*select @BagQuantity_AtEnd=BagQuantity, @Income=Income,@CorrectionValue=CorrectionValue,@CorrectionText=CorrectionText
from CorrectionsAtShiftEnd
where 
[DateShift]=@DateShift and 
[IsNight]=@IsNight AND 
[Material]=@Material
AND [BagWeight]=@BagWeight*/



DECLARE	@Loaded [int],
	@BagQuantity_at_start [int]=0,

	@DateShift_prv [date] =case @isnight when 1 then @DateShift else DATEADD(day,-1,@DateShift) end,
	@IsNight_prv [bit] =case @isnight when 1 then 0 else 1 end

	SELECT @DateShift_prv ,@IsNight_prv 
SELECT 
	  @Loaded=  Sum(t2.[BagsCount])
	  FROM
	  (
			SELECT 
				   Case When ([time]<'08:00:00') THEN Dateadd(day,-1,[date]) ELSE [date] END AS [DateShift]
				  ,Case When ([time]<'08:00:00' OR [time]>='20:00:00') THEN 1 ELSE 0 END AS [IsNight]
			      ,[Material]
			      ,[LUP]
			      ,[BagsCount]
			
			FROM (
					SELECT [Loaded]
					      ,[Material]
					      ,[LUP]
					      ,[BagsCount]
						  ,CONVERT (TIME,[Loaded]) as [time]
						  ,CONVERT (Date,[Loaded]) as [date]
					  FROM [dbo].[BagsLoadedCommand]
			  ) as t1
		) as t2
  group by 
       t2.[DateShift]
	  ,t2.[IsNight]
      ,t2.[Material]
      ,t2.[LUP]
having t2.[Material]=@Material AND t2.[DateShift]=@DateShift and t2.[IsNight]=@IsNight
	

select @BagQuantity_at_start=BagQuantity 
from CorrectionsAtShiftEnd
where 
[DateShift]=@DateShift_prv and 
[IsNight]=@IsNight_prv AND 
[Material]=@Material

INSERT INTO [dbo].[MaterialDataAtShiftEnd](
	--[MaterialDataAtShiftEndID] [int] IDENTITY(1,1) NOT NULL,
	[DateShift],
	[IsNight],
	[Material] ,
	[BagWeight] ,
	[BagQuantity_Start],
	[Income],
	[Loaded],
	[BagQuantity_End],
	[CorrectionValue],
	[CorrectionText] 
) VALUES(
@DateShift,
	@IsNight,
	@Material,
	@BagWeight,
	ISNULL(@BagQuantity_at_start,0),
	@Income,
	ISNULL(@loaded,0),
	@BagQuantity_AtEnd,
	@CorrectionValue,
	@CorrectionText 
)

GO