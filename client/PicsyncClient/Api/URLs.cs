using PicsyncClient.Enum;
using PicsyncClient.Models.UrlOptions;
using System.Diagnostics;

namespace PicsyncClient.Utils;

public static class URLs
{
    public static Uri BASE_URL => ServerData.Url;
    public static Uri API_URL => new(BASE_URL + "/api");
    public static Uri Settings => API_URL;


    public static Uri Albums => 
        new($"{API_URL}/albums");

    public static Uri AlbumInfo(ulong albumId) => 
        new($"{Albums}/{albumId}");


    public static Uri AlbumInvite(ulong albumId) =>
        new($"{AlbumInfo(albumId)}/invite");

    public static Uri Invitations =>
        new($"{API_URL}/invitation");

    public static Uri Invitation(string inviteCode) =>
        new($"{Invitations}/{inviteCode}");

    public static Uri InvitationAlbum(string inviteCode) =>
        new($"{Invitation(inviteCode)}/album");

    public static Uri InvitationJoin(string inviteCode) =>
        new($"{Invitation(inviteCode)}/join");

    public static Uri Accesses =>
        new($"{API_URL}/accesses");

    public static Uri AlbumAccess(ulong albumId, ulong? userId = null) =>
        new($"{AlbumInfo(albumId)}/accesses" + (userId != null ? $"/{userId}" : ""));


    public static Uri AlbumPictures(ulong albumId) => 
        new($"{AlbumInfo(albumId)}/pictures");

    public static Uri AlbumPictures(ulong albumId, AlbumPicturesOptions? options = null)
    {
        List<string> queryParams = [];

        if (options != null)
        {
            if (options.Page.HasValue)
                queryParams.Add($"page={options.Page.Value}");

            if (options.Limit.HasValue)
                queryParams.Add($"limit={options.Limit.Value}");

            if (options.Sort.HasValue)
                queryParams.Add($"sort={options.Sort.Value}");

            if (options?.IsReverse ?? false)
                queryParams.Add("reverse");
        }

        string queryString = queryParams.Any() 
            ? ("?" + string.Join("&", queryParams)) 
            : string.Empty;

        return new($"{AlbumInfo(albumId)}/pictures{queryString}");
    }

    public static Uri PictureInfo(ulong albumId, ulong pictureId) =>
        new($"{AlbumPictures(albumId)}/{pictureId}");

    public static Uri PictureOriginal(ulong albumId, ulong pictureId, string? signature = null)
    {
        string signatureString = (signature != null) 
            ? $"?sign={signature}" 
            : string.Empty;

        return new($"{PictureInfo(albumId, pictureId)}/original{signatureString}");
    }

    public static Uri PictureThumbnail(
        ulong          albumId         , 
        ulong          pictureId       , 
        string?        signature = null, 
        int            size      =  480, 
        SizeDirection? direction = null
    ) {
        string orient = direction switch
        {
            SizeDirection.Height => "h",
            SizeDirection.Width  => "w",
            _                    => "q",
        };

        string signatureString = (signature != null)
            ? $"?sign={signature}"
            : string.Empty;

        string str = $"{PictureInfo(albumId, pictureId)}/thumb/{orient}{size}{signatureString}";

        //Debug.WriteLine($"URLs: PictureThumbnail: {str}");
        return new(str);
    }
}