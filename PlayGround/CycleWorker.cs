namespace PlayGround;

/// <summary>
/// 用于执行不稳定且需要多次循环执行的逻辑的操作线程封装
/// </summary>
public class CycleWorker<T> : IDisposable
{
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
    private readonly Action<CycleWorkerStatus, T> _workerTask;

    /// <summary>
    /// 传入参数
    /// </summary>
    private T _parameter;

    /// <summary>
    /// 状态
    /// </summary>
    private CycleWorkerStatus _status;

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
    public CycleWorkerStatus Status => _status;

    /// <summary>
    /// ID
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// 工作线程异常退出时执行的Action
    /// </summary>
    public Action<Exception>? OnWorkerThreadException { get; set; }

    #endregion

    #region 构造方法

    /// <summary>
    /// 构造方法
    /// </summary>
    /// <param name="workerTask">需要执行的逻辑</param>
    /// <param name="initParameter">传入参数</param>
    /// <param name="cycle">执行次数，当值为-1时永远循环执行</param>
    /// <exception cref="ArgumentOutOfRangeException">cycle超出范围时抛出</exception>
    /// <exception cref="ArgumentNullException">传入空的执行逻辑时抛出</exception>
    public CycleWorker(Action<CycleWorkerStatus, T> workerTask, T initParameter, int cycle)
    {
        _status = new CycleWorkerStatus
        {
            State      = CycleWorkerState.Stopped,
            ErrorCount = 0,
            CycleCount = 0,
            Cycle      = cycle < -1 ? throw new ArgumentOutOfRangeException(nameof(cycle)) : cycle
        };
        _workerThread     = new Thread(ExecuteWork);
        Id                = Guid.NewGuid();
        _restartSemaphore = new Semaphore(0, 1);
        _workerTask       = workerTask ?? throw new ArgumentNullException(nameof(workerTask));
        _parameter        = initParameter;
    }

    #endregion

    #region 流程控制

    /// <summary>
    /// 启动工作线程
    /// </summary>
    /// <exception cref="NotSupportedException">非法的状态控制</exception>
    public void StartWorker()
    {
        switch (_status.State)
        {
            case CycleWorkerState.Running:
                throw new NotSupportedException("thread is already running!");
            case CycleWorkerState.Interrupted:
                _restartSemaphore.Release();
                break;
            case CycleWorkerState.Stopped:
                if (_isExecuted) ClearContext();
                else _isExecuted = true;
                _workerThread.Start();
                break;
            case CycleWorkerState.Disposed:
                throw new NotSupportedException("thread is already disposed!");
        }

        _status.State = CycleWorkerState.Running;
    }

    /// <summary>
    /// 启动工作线程
    /// </summary>
    /// <param name="parameter">覆盖原有参数</param>
    /// <exception cref="NotSupportedException">非法的状态控制</exception>
    public void StartWorker(T parameter)
    {
        _parameter = parameter;
        StartWorker();
    }

    /// <summary>
    /// 中断工作线程
    /// </summary>
    /// <exception cref="NotSupportedException">非法的状态控制</exception>
    public void InterruptWorker()
    {
        switch (_status.State)
        {
            case CycleWorkerState.Interrupted:
                throw new NotSupportedException("thread is already interrupted!");
            case CycleWorkerState.Stopped:
                throw new NotSupportedException("thread is already stopped!");
            case CycleWorkerState.Disposed:
                throw new NotSupportedException("thread is already disposed!");
        }

        _status.State = CycleWorkerState.Interrupted;
    }

    /// <summary>
    /// 停止工作线程
    /// </summary>
    /// <exception cref="NotSupportedException">非法的状态控制</exception>
    public void StopWorker()
    {
        switch (_status.State)
        {
            case CycleWorkerState.Interrupted:
                _restartSemaphore.Release();
                break;
            case CycleWorkerState.Stopped:
                throw new NotSupportedException("thread is already stopped!");
            case CycleWorkerState.Disposed:
                throw new NotSupportedException("thread is already disposed!");
        }

        _status.State = CycleWorkerState.Stopped;
        ClearContext();
    }

    /// <summary>
    /// Dispose
    /// </summary>
    /// <exception cref="NotSupportedException">非法的状态控制</exception>
    public void Dispose()
    {
        switch (_status.State)
        {
            case CycleWorkerState.Interrupted:
                _restartSemaphore.Release();
                break;
            case CycleWorkerState.Disposed:
                throw new NotSupportedException("thread is already disposed!");
        }

        _status.State = CycleWorkerState.Disposed;
        ClearContext(false);
        GC.SuppressFinalize(this);
    }

    #endregion

    #region 任务执行

    private void ExecuteWork(object? obj)
    {
        while (_status.Cycle == -1 || _status.CycleCount < _status.Cycle)
        {
            if (_status.State is CycleWorkerState.Interrupted)
                _restartSemaphore.WaitOne();
            if (_status.State is CycleWorkerState.Stopped or CycleWorkerState.Disposed)
                return;

            _status.CycleCount++;
            try
            {
                _workerTask.Invoke(_status, _parameter);
            }
            catch (Exception e)
            {
                if (OnWorkerThreadException is null) throw;
                OnWorkerThreadException.Invoke(e);
                _status.ExceptionCount++;
            }
        }

        ClearContext();
        _status.State = CycleWorkerState.Stopped;
    }

    #endregion

    #region 工具

    

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
    }

    #endregion

    #region 析构

    ~CycleWorker()
    {
        Dispose();
    }

    #endregion
}