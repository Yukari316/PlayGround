using System.Text;
using Newtonsoft.Json.Linq;

namespace PlayGround;

internal static class NcmDecryptUtil
{
    private static readonly byte[] CoreKey =
    {
        0x68, 0x7A, 0x48, 0x52, 0x41, 0x6D, 0x73, 0x6F, 0x35, 0x6B, 0x49, 0x6E, 0x62, 0x61, 0x78, 0x57
    };

    private static readonly byte[] MetaKey =
    {
        0x23, 0x31, 0x34, 0x6C, 0x6A, 0x6B, 0x5F, 0x21, 0x5C, 0x5D, 0x26, 0x30, 0x55, 0x3C, 0x27, 0x28
    };

    //Header
    internal static bool CheckHeader(this MemoryStream ms)
    {
        byte[] header = new byte[8];
        _ = ms.Read(header, 0, 8);
        return Encoding.UTF8.GetString(header) == "CTENFDAM";
    }

    //Key Box
    internal static byte[] MakeKeyBox(this MemoryStream ms)
    {
        uint keyboxLen = ms.ReadUint32();

        //raw keybox data
        byte[] buffer = new byte[keyboxLen];
        _ = ms.Read(buffer, 0, buffer.Length);
        for (int i = 0; i < buffer.Length; i++)
            buffer[i] ^= 0x64;

        //decrypt
        byte[] boxDataBytes = buffer.DecryptAES(CoreKey).Skip(17).ToArray();
        byte[] keybox       = Enumerable.Range(0, 256).Select(s => (byte) s).ToArray();
        byte   lastbyte     = 0;
        byte   offset       = 0;
        for (int i = 0; i < keybox.Length; i++)
        {
            byte c = (byte) ((keybox[i] + lastbyte + boxDataBytes[offset]) & 0xff);
            (keybox[i], keybox[c]) = (keybox[c], keybox[i]);

            lastbyte = c;
            offset++;
            if (offset >= boxDataBytes.Length)
                offset = 0;
        }

        return keybox;
    }

    //Meta Info
    internal static NetEaseMetaInfo ReadMeta(this MemoryStream ms)
    {
        uint   metaLen = ms.ReadUint32();
        byte[] buffer  = new byte[metaLen];
        _ = ms.Read(buffer, 0, buffer.Length);
        buffer = buffer.Skip(22).ToArray();
        for (int i = 0; i < buffer.Length; i++)
            buffer[i] ^= 0x63;

        buffer = Convert.FromBase64String(Encoding.UTF8.GetString(buffer));
        string metaJsonStr = Encoding.UTF8.GetString(buffer.DecryptAES(MetaKey)).Replace("music:", string.Empty);
        return JObject.Parse(metaJsonStr).ToObject<NetEaseMetaInfo>();
    }

    //Audio
    internal static byte[] ReadAudio(this MemoryStream ms, byte[] keyBox)
    {
        byte[]    buffer      = new byte[0x8000];
        using var audioStream = new MemoryStream();
        while (ms.Read(buffer, 0, buffer.Length) > 0)
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                int boxIndex = (i + 1) & 0xFF;
                buffer[i] ^= keyBox[(keyBox[boxIndex] + keyBox[(keyBox[boxIndex] + boxIndex) & 0xff]) & 0xff];
            }

            audioStream.Write(buffer, 0, buffer.Length);
        }

        return audioStream.ToArray();
    }
}