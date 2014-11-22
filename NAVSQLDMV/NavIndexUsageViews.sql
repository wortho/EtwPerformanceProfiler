-- Written by Jens Klarskov Jensen

if exists
(select top 1 null from INFORMATION_SCHEMA.VIEWS WHERE TABLE_NAME = 'IndexUsageView')
	drop view IndexUsageView
go
	
-- Index Usage Script 
CREATE VIEW IndexUsageView AS 
SELECT TOP 100 PERCENT 
		o.name AS ObjectName
	,	i.name AS IndexName
	,	i.index_id AS IndexId
	,	dm_ius.user_seeks AS UserSeeks
	,	dm_ius.user_scans AS UserScans
	,	dm_ius.user_lookups AS UserLookups
	,	dm_ius.user_updates AS UserUpdates
	,	p.TableRows
	, CASE WHEN (dm_ius.user_seeks + dm_ius.user_scans  + dm_ius.user_lookups = 0 AND dm_ius.user_updates > 0) THEN 
	'DROP INDEX ' + QUOTENAME(i.name) + ' ON ' + QUOTENAME(s.name) + '.' + QUOTENAME(OBJECT_NAME(dm_ius.object_id)) 
	ELSE '' END AS 'DropStatement'
 FROM sys.dm_db_index_usage_stats AS dm_ius
 INNER JOIN sys.indexes AS i 
	ON i.index_id = dm_ius.index_id AND dm_ius.object_id = i.object_id 
 INNER JOIN sys.objects AS o 
	ON dm_ius.object_id = o.object_id
 INNER JOIN sys.schemas AS s 
	ON o.schema_id = s.schema_id
 INNER JOIN 
 (SELECT SUM(p.rows) TableRows, p.index_id, p.object_id
  FROM sys.partitions AS p GROUP BY p.index_id, p.object_id) AS p 
	ON p.index_id = dm_ius.index_id 
	AND p.object_id = dm_ius.object_id
 WHERE OBJECTPROPERTY(dm_ius.object_id,'IsUserTable') = 1
 AND dm_ius.database_id = DB_ID()
 AND i.type_desc = 'nonclustered'
 AND i.is_primary_key = 0
 AND i.is_unique_constraint = 0
 ORDER BY (dm_ius.user_seeks + dm_ius.user_scans + dm_ius.user_lookups) ASC, UserUpdates DESC
 GO

 
SELECT * FROM IndexUsageView
--ORDER BY ObjectName, IndexName
ORDER BY (UserSeeks +UserScans + UserLookups) ASC, UserUpdates DESC
go

