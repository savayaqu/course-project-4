using PicsyncClient.Models.Albums;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace PicsyncClient.Models.Pictures;

public class PictureLocal : PictureBase, IPictureLocal
{
    // ������������
    [SetsRequiredMembers]
    public PictureLocal(IPictureLocal localPicture) : this()
    {
        LocalPath = localPicture.LocalPath;
        Album     = localPicture.Album;

        Name   = localPicture.Name;
        Hash   = localPicture.Hash;
        Size   = localPicture.Size;
        Width  = localPicture.Width;
        Height = localPicture.Height;
        Date   = localPicture.Date;
    }

    public PictureLocal() { }

    // ��������
    public override string Name => Path.GetFileName(LocalPath);
    public string LocalPath { get; set; }

    [JsonIgnore]
    public IAlbumLocal SpecificAlbum
    {
        get => (IAlbumLocal)Album;
        set => Album = value;
    }

    // �������
    public override string OriginalPath  => LocalPath;
    public override string ThumbnailPath => LocalPath;

    public override bool IsLocal => true;
    public override bool IsStrictLocal => true;
    public override bool IsLocalInSyncAlbum => Album is AlbumSynced;
}