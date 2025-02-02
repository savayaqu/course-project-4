using CommunityToolkit.Mvvm.ComponentModel;
using PicsyncClient.Models.Pictures;
using System.Diagnostics.CodeAnalysis;

namespace PicsyncClient.Models;

public partial class PictureDuplica()
{
    public PictureDuplica(string path) : this()
    {
        Path = path;
    }

    public string Path { get; set; }
}