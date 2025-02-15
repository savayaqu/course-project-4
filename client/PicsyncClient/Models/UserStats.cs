using CommunityToolkit.Mvvm.ComponentModel;
using PicsyncClient.Models.Response;
using System.Text.Json.Serialization;

namespace PicsyncClient.Models;

public partial class UserStats : ObservableObject
{
    [ObservableProperty] private List<Warning> warnings = [];
    [ObservableProperty] private int complaintsFromCount = 0;
    [ObservableProperty] private int complaintsFromAcceptedCount = 0;
    [ObservableProperty] private int albumsOwnCount = 0;
    [ObservableProperty] private int albumsAccessibleCount = 0;
    [ObservableProperty] private int picturesCount = 0;
    [ObservableProperty] private int tagsCount = 0;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UseQuota))]
    [NotifyPropertyChangedFor(nameof(FreeQuota))]
    private ulong totalQuota = 0;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UseQuota))]
    [NotifyPropertyChangedFor(nameof(FreeQuota))]
    private ulong usedQuota = 0;

    public DateTime GotAt { get; set; } = DateTime.Now; 

    public double UseQuota => (TotalQuota == 0) ? 0 : (1D / TotalQuota * UsedQuota);

    public ulong FreeQuota => TotalQuota - UsedQuota;

    public UserStats() { }

    public UserStats(UserWithStats userWithStats, SpaceQuota spaceQuota)
    {
        Warnings                    = userWithStats.Warnings;
        ComplaintsFromCount         = userWithStats.ComplaintsFromCount;
        ComplaintsFromAcceptedCount = userWithStats.ComplaintsFromAcceptedCount;
        AlbumsOwnCount              = userWithStats.AlbumsOwnCount;
        AlbumsAccessibleCount       = userWithStats.AlbumsAccessibleCount;
        PicturesCount               = userWithStats.PicturesCount;
        TagsCount                   = userWithStats.TagsCount;

        TotalQuota = spaceQuota.Total;
        UsedQuota  = spaceQuota.Used;
    }

    public void Update(UserWithStats userWithStats, SpaceQuota spaceQuota)
    {
        Warnings                    = userWithStats.Warnings;
        ComplaintsFromCount         = userWithStats.ComplaintsFromCount;
        ComplaintsFromAcceptedCount = userWithStats.ComplaintsFromAcceptedCount;
        AlbumsOwnCount              = userWithStats.AlbumsOwnCount;
        AlbumsAccessibleCount       = userWithStats.AlbumsAccessibleCount;
        PicturesCount               = userWithStats.PicturesCount;
        TagsCount                   = userWithStats.TagsCount;

        TotalQuota = spaceQuota.Total;
        UsedQuota  = spaceQuota.Used;

        GotAt = DateTime.Now;
    }
}