SELECT * FROM Users
SELECT * FROM Playlists WHERE Name like '%forza%'
SELECT * FROM Tracks
SELECT * FROM PlaylistDTOTrackDTO
SELECT * FROM PlaylistDTOUserDTO
SELECT * FROM Albums
SELECT * FROM Artists
SELECT * FROM AlbumDTOArtistDTO
SELECT * FROM Images

SELECT t.ID, t.Name, a.Name, a.ID FROM Tracks t LEFT JOIN Albums a ON a.ID = t.AlbumID ORDER BY a.Name
SELECT * FROM Tracks WHERE PlaylistDTOTrackDTO

UPDATE Playlists SET UserDTOID = NULL WHERE ID='00gS7nSgypzKL3hZNfxVre'

--DELETE FROM Users
--DELETE FROM Images
--DELETE FROM Playlists
--UPDATE Tracks SET AlbumID = NULL
--DELETE FROM Tracks
--DELETE FROM AlbumDTOArtistDTO
--DELETE FROM Albums
--DELETE FROM Artists
--DELETE FROM PlaylistDTOTrackDTO
--DELETE FROM PlaylistDTOUserDTO




/* Disconnect all clients
DECLARE @DatabaseName nvarchar(50) = N'SpotifyDB'
DECLARE @SQL varchar(max)
SELECT @SQL = COALESCE(@SQL,'') + 'Kill ' + Convert(varchar, SPId) + ';'
FROM MASTER..SysProcesses
WHERE DBId = DB_ID(@DatabaseName) AND SPId <> @@SPId
EXEC(@SQL)
*/