using TagLib;
using File = System.IO.File;

namespace PlayGround;

internal class FuckNcm
{
    public static async ValueTask<(bool success, string message)> FuckNcmFile(string path, string outPath)
    {
        await using var ms = new MemoryStream(await File.ReadAllBytesAsync(path));

        //检查文件头
        if (!ms.CheckHeader())
            return (false, "not a ncm file");

        ms.Seek(2, SeekOrigin.Current);

        //keybox
        byte[] box = ms.MakeKeyBox();

        //metainfo
        NetEaseMetaInfo meta = ms.ReadMeta();

        //ext name
        string ext = string.IsNullOrEmpty(meta.Format)
            ? "mp3"
            : meta.Format;

        var fileName = $"{Path.GetFileNameWithoutExtension(path)}.{ext}";

        //file exists
        if (File.Exists($@"{outPath.Trim()}\{fileName}")) return (false, "File exists");

        //crc32
        var crc32bytes = new byte[4];
        ms.Read(crc32bytes, 0, crc32bytes.Length);
        var crc32hash = $"0x{BitConverter.ToString(crc32bytes).Replace("-", string.Empty)}";

        //skip 5 character, 
        ms.Seek(5, SeekOrigin.Current);

        //cover
        uint imageLen = ms.ReadUint32();

        //cover
        byte[] imageBytes = new byte[imageLen];
        ms.Read(imageBytes, 0, imageBytes.Length);

        //audio
        byte[] audioBytes = ms.ReadAudio(box);

        //保存文件
        var outFilePath = $@"{outPath.Trim('\\')}\{fileName}";
        await File.WriteAllBytesAsync(outFilePath, audioBytes);

        //写入tag
        var tag = TagLib.File.Create(outFilePath);

        tag.Tag.Title      = meta.MusicName;
        tag.Tag.Performers = meta.Artist.Select(n => n[0].ToString()).ToArray();
        tag.Tag.Album      = meta.Album;
        tag.Tag.Subtitle   = string.Join(";", meta.Alias);

        //cover
        if (imageBytes.Length == 0)
        {
            (bool success, byte[] data) = 
                await Util.DownloadAlbumCoverAsync(meta.AlbumPic);

            imageBytes = success ? data : Array.Empty<byte>();
        }

        if (imageBytes.Length != 0)
        {
            var coverPic = new Picture(new ByteVector(imageBytes));
            tag.Tag.Pictures = new IPicture[] {coverPic};
        }

        tag.Save();

        return (true, $"{fileName}[{crc32hash}]");
    }
}