using PicsyncClient.Models.Pictures;
using SQLite;
using System.Diagnostics.CodeAnalysis;

namespace PicsyncClient.Models.Albums;

public class AlbumLocal : AlbumBase, IAlbumLocal
{
    // ������������
    [SetsRequiredMembers]
    public AlbumLocal(string path)
    {
        LocalPath = path;
    }

    [SetsRequiredMembers]
    public AlbumLocal(AlbumSynced noMoreSyncedAlbum)
    {
        LocalPath        = noMoreSyncedAlbum.LocalPath;
        LocalPictures    = noMoreSyncedAlbum.LocalPictures;
        NameDuplicaIndex = noMoreSyncedAlbum.NameDuplicaIndex;
    }

    public AlbumLocal() : base() { }

    // ��������
    public override string Name => Path.GetFileName(LocalPath);
    [Ignore] public int?       NameDuplicaIndex { get; set; } = null;
    public List<IPictureLocal> LocalPictures    { get; set; } = [];
    public required string     LocalPath        { get; set; }

    // �������
    public override int PicturesCount => LocalPictures.Count;

    private List<string>? _thumbnailPaths = null;
    public override List<string> ThumbnailPaths
    {
        // ������� ���������
        get
        {
            if (_thumbnailPaths != null) 
                return _thumbnailPaths;

            // �������������
            _thumbnailPaths = [];
            byte i = 0;
            foreach (var picture in LocalPictures)
            {
                if (i > 4) break;

                i++;
                if (picture.LocalPath is string path)
                    _thumbnailPaths.Add(path);
            }
            return _thumbnailPaths;
        }
    }
}
