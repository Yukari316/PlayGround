namespace PlayGround;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        //TODO 控制台进度条
        _ = args.GetParameter("i", out string? inputPath);
        bool hasOutput   = args.GetParameter("o", out string? outPath);
        bool copyAllFile = args.GetFlag("sync");

        if (!hasOutput) outPath = Path.GetDirectoryName(inputPath ?? "");

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

        var count = 1;

        foreach (string fileName in fileNames)
        {
            (bool success, string msg) = 
                await FuckNcm.FuckNcmFile(fileName, outPath);
            Console.WriteLine($"fuck ncm[{count++}/{fileNames.Length}]");
            if(success) Console.WriteLine($"ncm dump success[{msg}]");
            else
            {
                Console.WriteLine($"file [{fileName}] dump error({msg})");
                if (copyAllFile)
                {
                    Console.WriteLine("copy file to output dir");
                    string outputFile = $@"{outPath.Trim('\\')}\{Path.GetFileName(fileName)}";
                    File.Copy(fileName, outputFile);
                }
                else Console.WriteLine("ignore");
            }
        }

        Console.WriteLine($"ncm dump success[{outPath}]");

        Console.ReadKey();
        return 0;
    }
}