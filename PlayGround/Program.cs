using System;
using System.Text;

namespace PlayGround;

public static class Program
{
    //这个东西应该在管理池里,并且应该是ConcurrentDictionary
    public static Dictionary<Guid, CycleWorker<int>> CycleThreads = new();

    public static Timer timer = new((a) =>
                                    {
                                        //错误恢复模拟
                                        foreach ((Guid guid, CycleWorker<int>? cycleWorker) in
                                                 CycleThreads.Where(i => i.Value.Status.State ==
                                                                    CycleThreadState.Interrupted)
                                                        .ToList())
                                        {
                                            Console.WriteLine(cycleWorker.Status.WorkParameter + "已恢复");
                                            CycleThreads[guid].ClearError();
                                            CycleThreads[guid].Start();
                                        }

                                        foreach ((Guid key, CycleWorker<int>? value) in CycleThreads)
                                            //出错暂停模拟
                                            if (value.Status.TotalErrCount > 5 &&
                                                value.Status.State         == CycleThreadState.Running)
                                            {
                                                Console.WriteLine(value.Status.WorkParameter + "已停止");
                                                value.Interrupt();
                                            }
                                    },
                                    null,
                                    Timeout.Infinite,
                                    10000);

    public static void Main()
    {
        for (int i = 0; i < 4; i++)
        {
            var cThread = new CycleWorker<int>(para => { Console.WriteLine($"th [{para}] Start!"); },
                                               (ref CycleThreadStatus<int> status) =>
                                               {
                                                   Console
                                                       .WriteLine($"th [{status.WorkParameter}] run {status.CycleCount}");
                                               },
                                               (exception, status) =>
                                               {
                                                   Console
                                                       .WriteLine($"th [{status.WorkParameter}] err {exception.Message}");
                                               },
                                               i,
                                               TimeSpan.FromSeconds(1),
                                               true);
            CycleThreads[cThread.Id] = cThread;
        }

        timer.Change(0, 10000);
    }
}