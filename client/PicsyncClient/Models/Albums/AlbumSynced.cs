using PicsyncClient.Models.Pictures;
using PicsyncClient.Utils;
using SQLite;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace PicsyncClient.Models.Albums;

public class AlbumSynced : AlbumRemote, IAlbumLocal
{
    // ������������
    [SetsRequiredMembers]
    public AlbumSynced(IAlbumLocal local, AlbumRemote remote)
    {
        Id   = remote.Id;
        Name = remote.Name;

        LocalPath        = local.LocalPath;
        LocalPictures    = local.LocalPictures;
        NameDuplicaIndex = local.NameDuplicaIndex;
    }

    public AlbumSynced() : base() { }

    // ��������
    [Ignore]
    public int? NameDuplicaIndex { get; set; } = null;

    [Ignore]
    public List<IPictureLocal> LocalPictures { get; set; } = [];

    public  string LocalPath { get; set; }

    // �������
    public override int PicturesCount => (LocalPictures?.Count ?? 0) + RemotePicturesCount; // TODO: ����

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

            // TODO: ���������� API ����� ��������� �������� � ������, ����� ������� ���� ���������
            if (Preview is PictureAlbumPreview preview && preview.PictureIds.Count > 0)
            {
                foreach (var pictureId in preview.PictureIds)
                    _thumbnailPaths.Add(URLs.PictureThumbnail(Id, pictureId, preview.Signature).ToString());
            }


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

