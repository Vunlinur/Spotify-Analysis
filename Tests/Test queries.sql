SELECT * FROM Users
SELECT * FROM Playlists ORDER BY Name
SELECT * FROM PlaylistDTOUserDTO
SELECT * FROM Tracks
SELECT * FROM PlaylistDTOTrackDTO
SELECT * FROM Albums
SELECT * FROM Artists WHERE Name in ('Orden Ogan', 'Nightwish', 'Iron Maiden', 'Russell Brower')
SELECT * FROM ArtistDTOTrackDTO
SELECT * FROM AlbumDTOArtistDTO WHERE AlbumsID = '36Dk0lgHLB8nfpaC8EvGiy'
SELECT * FROM Images WHERE UserDTOID IS NOT NULL

-- All Tracks & their Albums
SELECT t.ID TrackID, t.Name TrackName, a.Name AlbumName, a.ID AlbumID FROM Tracks t LEFT JOIN Albums a ON a.ID = t.AlbumID ORDER BY a.Name
-- Playlist's Tracks, their Album & Artists
SELECT t.ID AS TrackID, t.Name AS TrackName, al.Name AS AlbumName, ar.Name AS AlbumArtist, ar.Genres as ArtistGenres, al.ReleaseDate, t.DurationMs, t.Popularity FROM Playlists p
JOIN PlaylistDTOTrackDTO pdt ON p.ID = pdt.PlaylistDTOID
JOIN Tracks t ON pdt.TracksID = t.ID
JOIN Albums al ON al.ID = t.AlbumID
JOIN AlbumDTOArtistDTO adt ON adt.AlbumsID = al.ID
JOIN Artists ar ON adt.ArtistsID = ar.ID
--WHERE p.Name LIKE '%test%'
ORDER BY ar.Name, al.Name

SELECT COUNT(*) FROM Images
SELECT * FROM Images WHERE Url = 'https://i.scdn.co/image/ab67616d00001e02135e3bce4f087e726fbb44f0'

UPDATE Users SET Updated = '2020' WHERE Name = 'Vunlinur'
UPDATE Playlists SET SnapshotID = '' WHERE ID = '7D8pgfx10524cS2i6kuKgo'


--DELETE FROM Images
--DELETE FROM Playlists
--DELETE FROM Users
--DELETE FROM Artists
--UPDATE Tracks SET AlbumID = NULL
--DELETE FROM Tracks
--DELETE FROM AlbumDTOArtistDTO
--DELETE FROM Albums
--DELETE FROM PlaylistDTOTrackDTO
--DELETE FROM PlaylistDTOUserDTO
--DELETE FROM ArtistDTOTrackDTO


/* Disconnect all clients
DECLARE @DatabaseName nvarchar(50) = N'SpotifyDB'
DECLARE @SQL varchar(max)
SELECT @SQL = COALESCE(@SQL,'') + 'Kill ' + Convert(varchar, SPId) + ';'
FROM MASTER..SysProcesses
WHERE DBId = DB_ID(@DatabaseName) AND SPId <> @@SPId
EXEC(@SQL)
*/