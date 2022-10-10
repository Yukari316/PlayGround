using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#pragma warning disable CS8618

namespace PlayGround;

internal struct NetEaseMetaInfo
{
    [JsonProperty(PropertyName = "musicId", NullValueHandling = NullValueHandling.Ignore)]
    public int MusicId { get; set; }

    [JsonProperty(PropertyName = "musicName", NullValueHandling = NullValueHandling.Ignore)]
    public string MusicName { get; set; }

    [JsonProperty(PropertyName = "artist", NullValueHandling = NullValueHandling.Ignore)]
    public List<JArray> Artist { get; set; }

    [JsonProperty(PropertyName = "albumId", NullValueHandling = NullValueHandling.Ignore)]
    public int AlbumId { get; set; }

    [JsonProperty(PropertyName = "album", NullValueHandling = NullValueHandling.Ignore)]
    public string Album { get; set; }

    [JsonProperty(PropertyName = "albumPicDocId", NullValueHandling = NullValueHandling.Ignore)]
    public string AlbumPicDocId { get; set; }

    [JsonProperty(PropertyName = "albumPic", NullValueHandling = NullValueHandling.Ignore)]
    public string AlbumPic { get; set; }

    [JsonProperty(PropertyName = "bitrate", NullValueHandling = NullValueHandling.Ignore)]
    public int Bitrate { get; set; }

    [JsonProperty(PropertyName = "mp3DocId", NullValueHandling = NullValueHandling.Ignore)]
    public string Mp3DocId { get; set; }

    [JsonProperty(PropertyName = "duration", NullValueHandling = NullValueHandling.Ignore)]
    public int Duration { get; set; }

    [JsonProperty(PropertyName = "mvId", NullValueHandling = NullValueHandling.Ignore)]
    public int MvId { get; set; }

    [JsonProperty(PropertyName = "alias", NullValueHandling = NullValueHandling.Ignore)]
    public List<string> Alias { get; set; }

    [JsonProperty(PropertyName = "transNames", NullValueHandling = NullValueHandling.Ignore)]
    public List<string> TransNames { get; set; }

    [JsonProperty(PropertyName = "format", NullValueHandling = NullValueHandling.Ignore)]
    public string Format { get; set; }
}