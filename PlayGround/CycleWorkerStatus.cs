namespace PlayGround;

public struct CycleWorkerStatus
{
    /// <summary>
    /// 错误计数
    /// 由用户控制
    /// </summary>
    public int ErrorCount { get; set; }

    /// <summary>
    /// 异常计数
    /// </summary>
    public int ExceptionCount { get; internal set; }

    /// <summary>
    /// 循环计数
    /// </summary>
    public int CycleCount { get; internal set; }

    /// <summary>
    /// <para>总循环次数</para>
    /// <para>值为-1时为无限循环</para>
    /// </summary>
    public int Cycle { get; internal init; }

    /// <summary>
    /// 状态
    /// </summary>
    public CycleWorkerState State { get; internal set; }

    /// <summary>
    /// 清空计数器
    /// </summary>
    public void ClearCount()
    {
        ErrorCount     = 0;
        CycleCount     = 0;
        ExceptionCount = 0;
    }
}