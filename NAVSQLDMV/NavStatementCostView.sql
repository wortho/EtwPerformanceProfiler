-- Written by Jens Klarskov Jensen

if exists
(select top 1 null from INFORMATION_SCHEMA.VIEWS WHERE TABLE_NAME = 'SqlStatementCost')
	drop view SqlStatementCost
go
	
-- SQL Statement costs
CREATE VIEW SqlStatementCost AS 
SELECT 
	SUBSTRING( qt.text, (qs.statement_start_offset/2)+1,
	(
	  (
		CASE qs.statement_end_offset
		WHEN -1 THEN DATALENGTH(qt.TEXT)
		ELSE qs.statement_end_offset
		END - qs.statement_start_offset)/2
	  ) + 1
	) AS StatementText
	, qs.execution_count AS ExectionCount
	, qs.total_elapsed_time as TotalElapsedTime
	, (qs.total_elapsed_time / qs.execution_count) as AverageElapsedTime
	, qs.last_elapsed_time as LastElapsedTime
	, qs.total_logical_reads AS TotalLogicalReads
	, qs.last_logical_reads AS LastLogicalReads
	, qs.total_logical_writes AS TotalLogicalWrites
	, qs.last_logical_writes AS LastLogicalWrites
	, qs.total_worker_time AS TotalWorkerTime
	, qs.last_worker_time AS LastWorkerTime
	, qp.query_plan AS QueryPlan
 FROM sys.dm_exec_query_stats AS qs
 CROSS APPLY sys.dm_exec_sql_text(qs.sql_handle) AS qt
 CROSS APPLY sys.dm_exec_query_plan(qs.plan_handle) AS qp
WHERE 
	qt.text NOT LIKE '%[sys].%' -- Not this query itself 
	AND qt.text like '%' + DB_NAME() + '%' 
go


SELECT * FROM SqlStatementCost
--ORDER BY ExectionCount DESC
--WHERE StatementText like '%INSERT INTO "DynamicsNAV80_Instance1".dbo."CRONUS International Ltd_$Sales Line"%'
ORDER BY AverageElapsedTime DESC
go