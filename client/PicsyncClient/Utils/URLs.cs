using Microsoft.Maui.ApplicationModel.DataTransfer;
using PicsyncClient.Enum;
using PicsyncClient.Models;
using System.Text.Json;

namespace PicsyncClient.Utils;

public static class URLs
{
    public readonly static Uri BASE_URL = new("https://sites.kopchan7.keenetic.link/picsync");
    public readonly static Uri  API_URL = new(BASE_URL + "/api");

    public static Uri Thumbnail(
        ulong          albumId        , 
        ulong          pictureId      , 
        string         signature      , 
        int            size      = 480, 
        SizeDirection? direction = null
    ) {
        string orient = direction switch
        {
            SizeDirection.Height => "h",
            SizeDirection.Width  => "w",
            _                    => "q",
        };
        return new Uri($"{API_URL}/albums/{albumId}/pictures/{pictureId}/thumb/{orient}{size}?sign={signature}");
    }
}