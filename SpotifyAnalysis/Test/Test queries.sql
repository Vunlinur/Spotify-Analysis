SELECT * FROM Users
SELECT * FROM Playlists WHERE Name like '%test%'
SELECT * FROM PlaylistDTOUserDTO
SELECT * FROM Tracks
SELECT * FROM PlaylistDTOTrackDTO
SELECT * FROM Albums
SELECT * FROM Artists WHERE ID = '6UJ3pPsN7xzsz0Cneucy59'
SELECT * FROM AlbumDTOArtistDTO WHERE AlbumsID = '36Dk0lgHLB8nfpaC8EvGiy'
SELECT * FROM Images

-- All Tracks & their Albums
SELECT t.ID TrackID, t.Name TrackName, a.Name AlbumName, a.ID AlbumID FROM Tracks t LEFT JOIN Albums a ON a.ID = t.AlbumID ORDER BY a.Name
-- Playlist's Tracks, their Album & Artists
SELECT t.ID AS TrackID, t.Name AS TrackName, ar.Name AS Artist, al.Name AS AlbumName, al.ReleaseDate, t.DurationMs, t.Popularity FROM Playlists p
JOIN PlaylistDTOTrackDTO pdt ON p.ID = pdt.PlaylistDTOID
JOIN Tracks t ON pdt.TracksID = t.ID
JOIN Albums al ON al.ID = t.AlbumID
JOIN AlbumDTOArtistDTO adt ON adt.AlbumsID = al.ID
JOIN Artists ar ON adt.ArtistsID = ar.ID
WHERE p.Name LIKE '%test%'
ORDER BY ar.Name, al.Name





--DELETE FROM Users
--DELETE FROM Images
--DELETE FROM Playlists
--DELETE FROM Artists
--UPDATE Tracks SET AlbumID = NULL
--DELETE FROM Tracks
--DELETE FROM AlbumDTOArtistDTO
--DELETE FROM Albums
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