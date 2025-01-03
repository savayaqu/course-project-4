namespace PicsyncClient.Models.Albums;

public abstract class AlbumBase : IAlbum
{
    public virtual string Name { get; set; }
    public abstract int PicturesCount { get; }
    public abstract List<string> ThumbnailPaths { get; }
}