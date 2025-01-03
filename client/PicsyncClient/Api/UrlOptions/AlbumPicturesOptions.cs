using PicsyncClient.Enum;

namespace PicsyncClient.Models.UrlOptions;

public class AlbumPicturesOptions
{
    public int?          Page        { get; set; }
    public int?          Limit       { get; set; }
    public PicturesSort? Sort        { get; set; }
    public bool?         IsReverse   { get; set; }
}