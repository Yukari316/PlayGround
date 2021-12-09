using System.Text;

namespace PlayGround;

public static class Program
{
    //这个东西应该在管理池里,并且应该是ConcurrentDictionary
    //public static Dictionary<Guid, CycleWorker<int>> Workers = new();

    public static void Main()
    {
        var worker = new CycleWorker<int>((status, i) =>
                                          {
                                              Console.WriteLine(status.CycleCount);
                                              Thread.Sleep(1000);
                                              if (status.CycleCount == 4) throw new Exception("死了啦都你害的啦");
                                          }, 0, -1);
        worker.OnWorkerThreadException = exception =>
                                         {
                                             Console.WriteLine(exception.Message);
                                         };
        //Workers.Add(worker.Id, worker);
        worker.StartWorker();
        Console.ReadLine();
        // worker.InterruptWorker();
        // Console.ReadLine();
        // worker.StartWorker();
        // Console.ReadLine();
    }
}