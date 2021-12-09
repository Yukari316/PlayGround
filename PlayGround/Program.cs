using System;
using System.Text;

namespace PlayGround;

public static class Program
{
    //这个东西应该在管理池里,并且应该是ConcurrentDictionary
    public static Dictionary<Guid, CycleWorker<string>> Workers = new();

    public static Timer timer = new Timer( ( a ) =>
                                           {
                                               //错误恢复模拟
                                               foreach ( ( Guid guid, CycleWorker<string>? cycleWorker ) in
                                                        Workers.Where( i => i.Value.Status.State ==
                                                                            CycleWorkerState.Interrupted )
                                                               .ToList() )
                                               {
                                                   Console.WriteLine( cycleWorker.Status.WorkParameter + "已恢复" );
                                                   Workers[ guid ].ClearError();
                                                   Workers[ guid ].Start();
                                               }

                                               foreach ( ( Guid key, CycleWorker<string>? value ) in Workers )
                                               {
                                                   //出错暂停模拟
                                                   if ( value.Status.TotalErrCount > 5 &&
                                                        value.Status.State == CycleWorkerState.Running )
                                                   {
                                                       Console.WriteLine( value.Status.WorkParameter + "已停止" );
                                                       value.Interrupt();
                                                   }
                                               }
                                           },
                                           null,
                                           Timeout.Infinite,
                                           10000 );

    public static void Main()
    {
        for ( int i = 0; i < 4; i++ )
        {
            var workerId = Guid.NewGuid();
            Workers[ workerId ] = new CycleWorker<string>( ( para ) =>
                                                           {
                                                               Console.WriteLine( para + "Start work!" );
                                                           },
                                                           ( ref CycleWorkerStatus<string> status ) =>
                                                           {
                                                               Console.WriteLine( $"I'm {status.WorkParameter}" );
                                                               status.ErrorCount++;
                                                           },
                                                           ( exception, status ) =>
                                                           {
                                                               Console.WriteLine( status.WorkParameter +
                                                                                  exception.Message );
                                                           },
                                                           $"test{i}-",
                                                           TimeSpan.FromSeconds( 1 ),
                                                           true );
        }

        timer.Change( 0, 10000 );
    }
}
