USE [Benraz.Authorization]
GO

CREATE OR ALTER PROCEDURE RoleClaim (
      @_ClaimId uniqueidentifier
	  ,@_CreateTimeUtc datetime2(7)
	  ,@_UpdateTimeUtc datetime2(7)
	  ,@_Type nvarchar(max)
	  ,@_Value nvarchar(max))
AS
BEGIN					 
   IF NOT EXISTS ( SELECT * FROM [dbo].[Claims] 
		             WHERE [Value] = @_Value)
   BEGIN     
	 INSERT INTO [dbo].[Claims] ([ClaimId], [CreateTimeUtc], [UpdateTimeUtc], [Type], [Value]) 
     VALUES (@_ClaimId, @_CreateTimeUtc, @_UpdateTimeUtc, @_Type , @_Value)
   END
END


DECLARE @_ClaimId uniqueidentifier = NEWID()
DECLARE @_CreateTimeUtc datetime2(7) = GETUTCDATE()
DECLARE @_UpdateTimeUtc datetime2(7)= GETUTCDATE()
DECLARE @_Type nvarchar(max) = 'claim'
DECLARE @_Value nvarchar(max)

set @_ClaimId = NEWID() set  @_CreateTimeUtc = GETUTCDATE() set @_UpdateTimeUtc = GETUTCDATE()  EXEC [dbo].[RoleClaim] @_ClaimId ,@_CreateTimeUtc ,@_UpdateTimeUtc ,@_Type, N'_MicroserviceTemplate_-mytables-read'
set @_ClaimId = NEWID() set  @_CreateTimeUtc = GETUTCDATE() set @_UpdateTimeUtc = GETUTCDATE()  EXEC [dbo].[RoleClaim] @_ClaimId ,@_CreateTimeUtc ,@_UpdateTimeUtc ,@_Type, N'_MicroserviceTemplate_-mytables-add'
set @_ClaimId = NEWID() set  @_CreateTimeUtc = GETUTCDATE() set @_UpdateTimeUtc = GETUTCDATE()  EXEC [dbo].[RoleClaim] @_ClaimId ,@_CreateTimeUtc ,@_UpdateTimeUtc ,@_Type, N'_MicroserviceTemplate_-mytables-update'
set @_ClaimId = NEWID() set  @_CreateTimeUtc = GETUTCDATE() set @_UpdateTimeUtc = GETUTCDATE()  EXEC [dbo].[RoleClaim] @_ClaimId ,@_CreateTimeUtc ,@_UpdateTimeUtc ,@_Type, N'_MicroserviceTemplate_-mytables-delete'

set @_ClaimId = NEWID() set  @_CreateTimeUtc = GETUTCDATE() set @_UpdateTimeUtc = GETUTCDATE()  EXEC [dbo].[RoleClaim] @_ClaimId ,@_CreateTimeUtc ,@_UpdateTimeUtc ,@_Type, N'_MicroserviceTemplate_-job-execute'
