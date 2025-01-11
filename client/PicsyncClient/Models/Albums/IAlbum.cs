namespace PicsyncClient.Models.Albums;

public interface IAlbum
{
    // Свойства
    public string Name { get; set; }

    // Геттеры
    public int PicturesCount { get; }
    public List<string> ThumbnailPaths { get; }
}