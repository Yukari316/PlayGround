using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using PlayGround.Properties;

namespace PlayGround;

public static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        DateTime s = DateTime.Now;
        //音频读取
        NcmFileInfo ncmFile = FuckNcm.FuckNcmFile(args[0]);

        //await using AudioFileReader reader = new AudioFileReader(args[0]);
        StreamMediaFoundationReader reader = new StreamMediaFoundationReader(ncmFile.DataStream);
        // WaveFileReader              reader = new WaveFileReader(ncmFile.DataStream);
        // reader.Volume = 0.05f;
        // Console.WriteLine($"time:{reader.TotalTime}");
        // Console.WriteLine($"file format:{reader.WaveFormat}");
        // Console.WriteLine($"br:{reader.Length * .008 / reader.TotalTime.TotalSeconds}");

        //重采样
        // WaveFormat waveFormat = new WaveFormat(384000, reader.WaveFormat.BitsPerSample, reader.WaveFormat.Channels);

        //Media Foundation Resampler
        // using MediaFoundationResampler mfResampler = new(reader, waveFormat);

        //Wdl Resampling Sample Provider
        // WdlResamplingSampleProvider wdlResampler = new(reader, 384000);

        #region ASIO

        // foreach (var asio in AsioOut.GetDriverNames())
        //     Console.WriteLine(asio);

        // AsioOut asioDevice = new AsioOut("ASIO Combo384 Driver");
        // asioDevice.InputChannelOffset = asioDevice.DriverInputChannelCount;
        // asioDevice.Init(mfResampler);
        // DateTime e = DateTime.Now;
        // Console.WriteLine();
        // Console.WriteLine($"time = {(e - s).TotalMilliseconds} ms");
        // asioDevice.Play();

        #endregion

        #region WASAPI

        // var enumerator = new MMDeviceEnumerator();
        // foreach (MMDevice wasapi in enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
        //     Console.WriteLine($"{wasapi.DataFlow} {wasapi.FriendlyName} {wasapi.DeviceFriendlyName} {wasapi.State}");
        //
        using WasapiOut wasapiOut = new WasapiOut();
        wasapiOut.Init(reader);
        wasapiOut.Play();
        DateTime e = DateTime.Now;
        Console.WriteLine(wasapiOut.OutputWaveFormat);
        Console.WriteLine($"time = {(e - s).TotalMilliseconds} ms");

        #endregion


        #region WaveOut API

        // for (int n = -1; n < WaveOut.DeviceCount; n++)
        // {
        //     var caps = WaveOut.GetCapabilities(n);
        //     Console.WriteLine($"{n}: {caps.ProductName}");
        // }
        //
        // using WaveOutEvent waveOut = new WaveOutEvent();
        // waveOut.Init(wdlResampler);
        //
        // waveOut.Play();
        //
        // Console.WriteLine($"v ={reader.Volume}");
        // Console.WriteLine(waveOut.OutputWaveFormat);

        #endregion

        #region DirectSound API

        // List<Guid> dsOutDevice = new();
        // foreach (var dev in DirectSoundOut.Devices)
        // {
        //     dsOutDevice.Add(dev.Guid);
        //     Console.WriteLine($"{dev.Guid} {dev.ModuleName} {dev.Description}");
        // }
        //
        // using DirectSoundOut drOut = new DirectSoundOut();
        // drOut.Init(reader);
        // drOut.Play();

        #endregion


        while (true)
        {
            Console.ReadKey();
            // stream.Volume -= 0.001f;
            Console.WriteLine("v -0.001");
            // Console.WriteLine($"v ={stream.Volume}");
        }
        // Console.ReadKey();
    }
}