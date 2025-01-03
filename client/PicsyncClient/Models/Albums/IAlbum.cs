namespace PicsyncClient.Models.Albums;

public interface IAlbum
{
    public string Name { get; set; }
    public int PicturesCount { get; }
    public List<string> ThumbnailPaths { get; }
}