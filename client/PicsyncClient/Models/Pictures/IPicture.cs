using PicsyncClient.Models.Albums;

namespace PicsyncClient.Models.Pictures;

public interface IPicture
{
    // Свойства
    public string    Name   { get; set; }
    public string    Hash   { get; set; }
    public ulong     Size   { get; set; }
    public int       Width  { get; set; }
    public int       Height { get; set; }
    public DateTime  Date   { get; set; }
    public IAlbum    Album  { get; set; }

    // Геттеры
    public string OriginalPath  { get; }
    public string ThumbnailPath { get; }

    public bool IsLocal          { get; }
    public bool IsSynced         { get; }
    public bool IsRemote         { get; }
    public bool IsRemoteNonOwned { get; }
    public bool IsRemoteOwned    { get; }
}