using PicsyncClient.Models.Albums;
using SQLite;

namespace PicsyncClient.Models.Pictures;

public abstract class PictureBase : IPicture
{
    // Свойства
    public virtual string    Name   { get; set; }
    public virtual string?   Hash   { get; set; }
    public virtual ulong?    Size   { get; set; }
    public virtual int?      Width  { get; set; }
    public virtual int?      Height { get; set; }
    public virtual DateTime? Date   { get; set; }

    [Ignore]
    public virtual IAlbum    Album  { get; set; }
           
    // Геттеры
    public abstract string OriginalPath  { get; }
    public abstract string ThumbnailPath { get; }
}