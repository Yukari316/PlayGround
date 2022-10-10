using NAudio.Wave;
using PlayGround.Properties;
using TagLib;
using File = System.IO.File;
using Tag = TagLib.Id3v2.Tag;

namespace PlayGround;

internal class FuckNcm
{
    public static NcmFileInfo FuckNcmFile(string path)
    {
        using var ms = new MemoryStream(File.ReadAllBytes(path));

        //检查文件头
        if (!ms.CheckHeader())
            throw new NotSupportedException("not a ncm file");

        ms.Seek(2, SeekOrigin.Current);

        //keybox
        byte[] box = ms.MakeKeyBox();

        //metainfo
        NetEaseMetaInfo meta = ms.ReadMeta();
        //ext name
        string ext = string.IsNullOrEmpty(meta.Format)
            ? "mp3"
            : meta.Format;

        //crc32
        var crc32bytes = new byte[4];
        ms.Read(crc32bytes, 0, crc32bytes.Length);

        //skip 5 character, 
        ms.Seek(5, SeekOrigin.Current);

        //cover
        uint imageLen = ms.ReadUint32();

        //cover
        byte[] imageBytes = new byte[imageLen];
        ms.Read(imageBytes, 0, imageBytes.Length);

        //audio
        byte[] audioBytes = ms.ReadAudio(box);

        //写入tag
        Tag tags = new Tag();

        tags.Title      = meta.MusicName;
        tags.Performers = meta.Artist.Select(n => n[0].ToString()).ToArray();
        tags.Album      = meta.Album;
        tags.Subtitle   = string.Join(";", meta.Alias);

        //cover
        if (imageBytes.Length == 0)
        {
            imageBytes = Array.Empty<byte>();
        }

        if (imageBytes.Length != 0)
        {
            var coverPic = new Picture(new ByteVector(imageBytes));
            tags.Pictures = new IPicture[] {coverPic};
        }

        return new NcmFileInfo
        {
            Tags       = tags,
            Ext        = ext,
            DataStream = new MemoryStream(audioBytes)
        };
    }
}