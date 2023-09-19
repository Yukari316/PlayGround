namespace PlayGround;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        if (args.Length != 1)
        {
            return -1;
        }
        string inputPath = args[0];
        string outPath = $"{Environment.CurrentDirectory}/output";
        if (!Directory.Exists(outPath))
            Directory.CreateDirectory(outPath);

        if (!(!string.IsNullOrEmpty(inputPath) && !string.IsNullOrEmpty(outPath)))
        {
            Console.WriteLine("illegal input");
            return -2;
        }

        FileAttributes inputAttr;
        FileAttributes outputAttr;
        try
        {
            inputAttr  = File.GetAttributes(inputPath);
            outputAttr = File.GetAttributes(outPath);
        }
        catch (Exception e)
        {
            Console.WriteLine($"cannot get file info[{e.Message}]");
            return -3;
        }
        if (!outputAttr.HasFlag(FileAttributes.Directory))
        {
            Console.WriteLine("output path is not a directory");
            return -1;
        }

        //File
        if (!inputAttr.HasFlag(FileAttributes.Directory))
        {
            (bool success, string msg) = await FuckNcm.FuckNcmFile(inputPath, outPath);

            if (success)
            {
                Console.WriteLine($"ncm dump success[{msg}]");
                return 0;
            }
            Console.WriteLine($"ncm dump failed[{msg}]");

            return -4;
        }
        //Dir
        string[] fileNames = Directory.GetFiles(inputPath);

        int count = 1;

        foreach (string fileName in fileNames)
        {
            (bool success, string msg) = 
                await FuckNcm.FuckNcmFile(fileName, outPath);
            Console.WriteLine($"fuck ncm[{count++}/{fileNames.Length}]");
            Console.WriteLine(success ? $"ncm dump success[{msg}]" : $"file [{fileName}] dump error({msg})");
        }

        Console.WriteLine($"ncm dump success[{outPath}]");

        Console.ReadKey();
        return 0;
    }
}