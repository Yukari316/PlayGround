using System.Net;
using System.Security.Cryptography;

namespace PlayGround;

internal static class Util
{
    internal static uint ReadUint32(this MemoryStream ms, int offset = 0)
    {
        byte[] buffer = new byte[4];
        ms.Read(buffer, offset, buffer.Length);
        return BitConverter.ToUInt32(buffer);
    }

    internal static byte[] DecryptAES(this byte[] data, byte[] key)
    {
        using Aes aes = Aes.Create();
        aes.Key  = key;
        aes.Mode = CipherMode.ECB;
        ICryptoTransform decrypter = aes.CreateDecryptor();
        return decrypter.TransformFinalBlock(data, 0, data.Length);
    }

    internal static async ValueTask<(bool success, byte[] data)> DownloadAlbumCoverAsync(string url)
    {
        var client = new HttpClient();
        try
        {
            var res = await client.GetAsync(url);
            if (res.StatusCode != HttpStatusCode.OK)
                return (false, Array.Empty<byte>());
            await using Stream stream    = await res.Content.ReadAsStreamAsync();
            await using var    memStream = new MemoryStream();
            await stream.CopyToAsync(memStream);
            memStream.Position = 0;
            return (true, memStream.ToArray());
        }
        catch
        {
            return (false, Array.Empty<byte>());
        }
    }
}