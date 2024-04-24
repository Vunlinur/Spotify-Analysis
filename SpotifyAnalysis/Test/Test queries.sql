SELECT * FROM Users
SELECT * FROM Playlists WHERE Name like '%Turnau%'

UPDATE Playlists SET UserDTOID = NULL WHERE ID='00gS7nSgypzKL3hZNfxVre'

--DELETE FROM Playlists
--DELETE TOP(10) FROM Playlists




/* Disconnect all clients
DECLARE @DatabaseName nvarchar(50) = N'SpotifyDB'
DECLARE @SQL varchar(max)
SELECT @SQL = COALESCE(@SQL,'') + 'Kill ' + Convert(varchar, SPId) + ';'
FROM MASTER..SysProcesses
WHERE DBId = DB_ID(@DatabaseName) AND SPId <> @@SPId
EXEC(@SQL)
*/