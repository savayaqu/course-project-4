using PicsyncClient.Models.Albums;
using SQLite;

namespace PicsyncClient.Models.Pictures;

public abstract class PictureBase : IPicture
{
    // Свойства
    public virtual string    Name   { get; set; }
    public virtual string    Hash   { get; set; }
    public virtual ulong     Size   { get; set; }
    public virtual int       Width  { get; set; }
    public virtual int       Height { get; set; }
    public virtual DateTime  Date   { get; set; }

    [Ignore]
    public virtual IAlbum    Album  { get; set; }
           
    // Геттеры
    public abstract string OriginalPath  { get; }
    public abstract string ThumbnailPath { get; }
    
    public virtual bool IsSynced               => false;

    public virtual bool IsLocal                => false;
    public virtual bool IsStrictLocal          => false;
    public virtual bool IsLocalInSyncAlbum     => false;

    public virtual bool IsRemote               => false;
    public virtual bool IsStrictRemote         => false;
    public virtual bool IsRemoteInSyncAlbum    => false;
    public virtual bool IsRemoteNonOwned       => false;
    public virtual bool IsRemoteOwned          => false;
    public virtual bool IsStrictRemoteNonOwned => false;
    public virtual bool IsStrictRemoteOwned    => false;
}