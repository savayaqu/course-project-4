using PicsyncClient.Models.Pictures;
using SQLite;

namespace PicsyncClient.Models.Albums;

public interface IAlbumLocal : IAlbum
{
    // Свойства
    [Ignore] public int?      NameDuplicaIndex { get; set; }
    public List<PictureLocal> LocalPictures    { get; set; }
    public string             LocalPath        { get; set; }
}