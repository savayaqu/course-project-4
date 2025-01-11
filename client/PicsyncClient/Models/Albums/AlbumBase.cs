namespace PicsyncClient.Models.Albums;

public abstract class AlbumBase : IAlbum
{
    // Свойства
    public virtual string Name { get; set; }

    // Геттеры
    public abstract int PicturesCount { get; }
    public abstract List<string> ThumbnailPaths { get; }
}