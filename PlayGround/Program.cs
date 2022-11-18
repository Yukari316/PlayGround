using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Newtonsoft.Json;
using PlayGround.Properties;

namespace PlayGround;

public static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        foreach (string s1 in args)
        {
            Console.WriteLine(s1);
        }
        DateTime s = DateTime.Now;
        //音频读取
        NcmFileInfo ncmFile = FuckNcm.FuckNcmFile(args[0]);
        
        // using AudioFileReader reader = new(args[0]);
        StreamMediaFoundationReader reader = new(ncmFile.DataStream);
        // WaveFileReader              reader = new WaveFileReader(ncmFile.DataStream);


        //重采样
        WaveFormat waveFormat = new WaveFormat(384000, reader.WaveFormat.BitsPerSample, reader.WaveFormat.Channels);

        //Media Foundation Resampler
        using MediaFoundationResampler mfResampler = new(reader, waveFormat);

        //Wdl Resampling Sample Provider
        // WdlResamplingSampleProvider wdlResampler = new(reader, 384000);

        #region ASIO

        // foreach (var asio in AsioOut.GetDriverNames())
        //     Console.WriteLine(asio);

        // AsioOut asioDevice = new AsioOut("ASIO Combo384 Driver");
        // asioDevice.InputChannelOffset = asioDevice.DriverInputChannelCount;
        // asioDevice.Init(wdlResampler);
        // DateTime e = DateTime.Now;
        // Console.WriteLine();
        // Console.WriteLine($"time = {(e - s).TotalMilliseconds} ms");
        // asioDevice.Play();

        #endregion

        #region WASAPI

        // var enumerator = new MMDeviceEnumerator();
        // foreach (MMDevice wasapi in enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
        // {
        //     Console.WriteLine($"{wasapi.DataFlow} | {wasapi.FriendlyName} | {wasapi.DeviceFriendlyName} | {wasapi.State} | {wasapi.ID}");
        //     Guid id = Guid.Parse(wasapi.ID.Split("}.{")[1][..^1]);
        //     Console.WriteLine(id);
        // }
        //
        // ;
        //
        WasapiOut wasapiOut = new WasapiOut();
        wasapiOut.Init(mfResampler);
        wasapiOut.Play();
        wasapiOut.Volume = 0.01f;

        DateTime e = DateTime.Now;
        Console.WriteLine(wasapiOut.OutputWaveFormat);
        Console.WriteLine($"time = {(e - s).TotalMilliseconds} ms");
        Console.ReadKey();

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
        // //
        // using var drOut = new DirectSoundOut();
        // drOut.Init(wdlResampler);
        // drOut.Play();

        #endregion


        // while (true)
        // {
        //     Console.ReadKey();
        //     reader.Volume -= 0.001f;
        //     Console.WriteLine("v -0.001");
        //     Console.WriteLine($"v ={reader.Position}");
        // }
        // Console.ReadKey();
    }
}