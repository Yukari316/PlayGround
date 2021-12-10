using System.Text;

namespace PlayGround;

/// <summary>
/// 用于执行需要多次循环执行的逻辑的操作线程封装
/// </summary>
public class CycleWorker<TData> : IDisposable
{
    public delegate void ActionRef<T>(ref T item);


    #region 管理字段

    /// <summary>
    /// 工作线程
    /// </summary>
    private Thread _workerThread;

    /// <summary>
    /// 中断控制信号量
    /// </summary>
    private Semaphore _restartSemaphore;

    /// <summary>
    /// 需要执行Action
    /// </summary>
    private readonly ActionRef<CycleThreadStatus<TData>> _workerTask;

    /// <summary>
    /// 状态
    /// </summary>
    private CycleThreadStatus<TData> _status;

    /// <summary>
    /// 已经执行过至少一次
    /// </summary>
    private bool _isExecuted;

    #endregion

    #region worker信息

    /// <summary>
    /// 所在线程的状态
    /// </summary>
    public ThreadState ThreadState => _workerThread.ThreadState;

    /// <summary>
    /// CycleWorker状态
    /// </summary>
    public CycleThreadStatus<TData> Status => _status;

    /// <summary>
    /// ID
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// 工作线程异常退出时执行的Action
    /// </summary>
    public Action<Exception, CycleThreadStatus<TData>>? OnWorkerThreadException { get; set; }

    /// <summary>
    /// 两次执行的间隔时间，若为-1则不生效（单位：毫秒）
    /// </summary>
    public int Interval { get; set; } = -1;

    #endregion

    #region 构造方法

    /// <summary>
    /// 构造方法
    /// </summary>
    /// <param name="workerTask">需要执行的逻辑</param>
    /// <param name="initParameter">传入参数</param>
    /// <param name="isStartImmediately">是否立即启动</param>
    /// <param name="cycle">执行次数，当值为-1时永远循环执行</param>
    /// <param name="interval">两次执行之间间隔时间，当值为null时不生效</param>
    /// <exception cref="ArgumentOutOfRangeException">cycle超出范围时抛出</exception>
    /// <exception cref="ArgumentNullException">传入空的执行逻辑时抛出</exception>
    public CycleWorker(ActionRef<CycleThreadStatus<TData>> workerTask,
                       TData initParameter,
                       TimeSpan? interval = null,
                       bool isStartImmediately = false,
                       int cycle = -1)
        : this(null, workerTask, null, initParameter, interval, isStartImmediately, cycle)
    {
    }

    /// <summary>
    /// 构造方法
    /// </summary>
    /// <param name="initTask">初始化逻辑</param>
    /// <param name="workerTask">需要执行的逻辑</param>
    /// <param name="initParameter">传入参数</param>
    /// <param name="isStartImmediately">是否立即启动</param>
    /// <param name="cycle">执行次数，当值为-1时永远循环执行</param>
    /// <param name="interval">两次执行之间间隔时间，当值为null时不生效</param>
    /// <exception cref="ArgumentOutOfRangeException">cycle超出范围时抛出</exception>
    /// <exception cref="ArgumentNullException">传入空的执行逻辑时抛出</exception>
    public CycleWorker(Action<TData> initTask,
                       ActionRef<CycleThreadStatus<TData>> workerTask,
                       TData initParameter,
                       TimeSpan? interval = null,
                       bool isStartImmediately = false,
                       int cycle = -1)
        : this(initTask, workerTask, null, initParameter, interval, isStartImmediately, cycle)
    {
    }

    /// <summary>
    /// 构造方法
    /// </summary>
    /// <param name="workerTask">需要执行的逻辑</param>
    /// <param name="exceptionTask">出错后的处理逻辑</param>
    /// <param name="initParameter">传入参数</param>
    /// <param name="isStartImmediately">是否立即启动</param>
    /// <param name="cycle">执行次数，当值为-1时永远循环执行</param>
    /// <param name="interval">两次执行之间间隔时间，当值为null时不生效</param>
    /// <exception cref="ArgumentOutOfRangeException">cycle超出范围时抛出</exception>
    /// <exception cref="ArgumentNullException">传入空的执行逻辑时抛出</exception>
    public CycleWorker(ActionRef<CycleThreadStatus<TData>> workerTask,
                       Action<Exception, CycleThreadStatus<TData>> exceptionTask,
                       TData initParameter,
                       TimeSpan? interval = null,
                       bool isStartImmediately = false,
                       int cycle = -1)
        : this(null, workerTask, exceptionTask, initParameter, interval, isStartImmediately, cycle)
    {
    }

    /// <summary>
    /// 构造方法
    /// </summary>
    /// <param name="initTask">初始化逻辑</param>
    /// <param name="workerTask">需要执行的逻辑</param>
    /// <param name="exceptionTask">出错后的处理逻辑</param>
    /// <param name="initParameter">传入参数</param>
    /// <param name="isStartImmediately">是否立即启动</param>
    /// <param name="cycle">执行次数，当值为-1时永远循环执行</param>
    /// <param name="interval">两次执行之间间隔时间，当值为null时不生效</param>
    /// <exception cref="ArgumentOutOfRangeException">cycle超出范围时抛出</exception>
    /// <exception cref="ArgumentNullException">传入空的执行逻辑时抛出</exception>
    public CycleWorker(Action<TData>? initTask,
                       ActionRef<CycleThreadStatus<TData>> workerTask,
                       Action<Exception, CycleThreadStatus<TData>>? exceptionTask,
                       TData initParameter,
                       TimeSpan? interval = null,
                       bool isStartImmediately = false,
                       int cycle = -1)
    {
        _status = new CycleThreadStatus<TData>
        {
            State         = CycleThreadState.Stopped,
            ErrorCount    = 0,
            CycleCount    = 0,
            Cycle         = cycle < -1 ? throw new ArgumentOutOfRangeException(nameof(cycle)) : cycle,
            WorkParameter = initParameter
        };
        _workerThread           = new Thread(ExecuteWork);
        Id                      = Guid.NewGuid();
        Interval                = interval is null ? -1 : Convert.ToInt32(interval.Value.TotalMilliseconds);
        OnWorkerThreadException = exceptionTask;
        _restartSemaphore       = new Semaphore(0, 1);
        _workerTask             = workerTask ?? throw new ArgumentNullException(nameof(workerTask));

        initTask?.Invoke(_status.WorkParameter);

        if (isStartImmediately)
            Start();
    }

    #endregion

    #region 流程控制

    /// <summary>
    /// 启动工作线程
    /// </summary>
    /// <exception cref="InvalidOperationException">非法的状态控制</exception>
    public void Start()
    {
        switch (_status.State)
        {
            case CycleThreadState.Running:
                throw new InvalidOperationException("thread is already running!");
            case CycleThreadState.Interrupted:
                _status.State = CycleThreadState.Running;
                _restartSemaphore.Release();
                break;
            case CycleThreadState.Stopped:
                _status.State = CycleThreadState.Running;
                if (_isExecuted) ClearContext();
                else _isExecuted = true;
                _workerThread.Start();
                break;
            case CycleThreadState.Disposed:
                throw new InvalidOperationException("thread is already disposed!");
        }
        // var sb = new StringBuilder();
        // sb.AppendLine("--------------------------");
        // sb.AppendLine($"th {_workerThread.ManagedThreadId} start");
        // sb.AppendLine($"th state: {_workerThread.ThreadState}");
        // sb.AppendLine("--------------------------");
        // Console.Write(sb.ToString());
    }

    /// <summary>
    /// 启动工作线程
    /// </summary>
    /// <param name="parameter">覆盖原有参数</param>
    /// <exception cref="InvalidOperationException">非法的状态控制</exception>
    public void Start(TData parameter)
    {
        _status.WorkParameter = parameter;
        Start();
    }

    /// <summary>
    /// 中断工作线程
    /// </summary>
    /// <exception cref="InvalidOperationException">非法的状态控制</exception>
    public void Interrupt()
    {
        switch (_status.State)
        {
            case CycleThreadState.Interrupted:
                throw new InvalidOperationException("thread is already interrupted!");
            case CycleThreadState.Stopped:
                throw new InvalidOperationException("thread is already stopped!");
            case CycleThreadState.Disposed:
                throw new InvalidOperationException("thread is already disposed!");
        }
        _status.State = CycleThreadState.Interrupted;
    }

    /// <summary>
    /// 停止工作线程
    /// </summary>
    /// <exception cref="InvalidOperationException">非法的状态控制</exception>
    public void Stop()
    {
        switch (_status.State)
        {
            case CycleThreadState.Interrupted:
                _status.State = CycleThreadState.Stopped;
                _restartSemaphore.Release();
                break;
            case CycleThreadState.Stopped:
                throw new InvalidOperationException("thread is already stopped!");
            case CycleThreadState.Disposed:
                throw new InvalidOperationException("thread is already disposed!");
        }
        ClearContext();
    }

    /// <summary>
    /// Dispose
    /// </summary>
    /// <exception cref="InvalidOperationException">非法的状态控制</exception>
    public void Dispose()
    {
        switch (_status.State)
        {
            case CycleThreadState.Interrupted:
                _status.State = CycleThreadState.Disposed;
                _restartSemaphore.Release();
                break;
            case CycleThreadState.Disposed:
                throw new InvalidOperationException("thread is already disposed!");
        }
        ClearContext(false);
        GC.SuppressFinalize(this);
    }

    #endregion

    #region 任务执行

    private void ExecuteWork(object? obj)
    {
        while (_status.Cycle == -1 || _status.CycleCount < _status.Cycle)
        {
            if (_status.State is CycleThreadState.Interrupted)
                _restartSemaphore.WaitOne();
            if (_status.State is CycleThreadState.Stopped or CycleThreadState.Disposed)
                return;

            _status.CycleCount++;
            try
            {
                _workerTask.Invoke(ref _status);
            }
            catch (Exception e)
            {
                if (OnWorkerThreadException is null) throw;
                OnWorkerThreadException.Invoke(e, _status);
                _status.ExceptionCount++;
            }

            if (Interval >= 0) Thread.Sleep(Interval);
        }

        ClearContext();
        _status.State = CycleThreadState.Stopped;
    }

    #endregion

    #region 工具

    /// <summary>
    /// 清空所有错误计数器
    /// </summary>
    public void ClearError()
    {
        _status.ClearError();
    }

    /// <summary>
    /// 清空循环计数器
    /// </summary>
    public void ClearCount()
    {
        _status.ClearCount();
    }

    /// <summary>
    /// 清空当前上下文
    /// </summary>
    /// <param name="renewThread">在正常停止时创建新的线程</param>
    private void ClearContext(bool renewThread = true)
    {
        if (renewThread)
        {
            _workerThread     = new Thread(ExecuteWork);
            _restartSemaphore = new Semaphore(0, 1);
        }

        _status.ClearCount();
        _status.ClearError();
    }

    #endregion

    #region 析构

    ~CycleWorker()
    {
        Dispose();
    }

    #endregion
}