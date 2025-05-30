
CREATE TABLE [dbo].[VoucherMaster] (
    [VoucherId]   INT           IDENTITY (1, 1) NOT NULL,
    [VoucherType] VARCHAR (20)  NOT NULL,
    [VoucherDate] DATE          NOT NULL,
    [ReferenceNo] VARCHAR (50)  NOT NULL,
    [CreatedBy]   VARCHAR (100) NOT NULL,
    [CreatedDate] DATETIME      DEFAULT (getdate()) NOT NULL,
    PRIMARY KEY CLUSTERED ([VoucherId] ASC)
);

