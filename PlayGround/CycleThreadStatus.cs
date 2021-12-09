namespace PlayGround;

public struct CycleThreadStatus<T>
{
    /// <summary>
    /// 总共错误数（异常数+用户定义的错误计数）
    /// </summary>
    public int TotalErrCount => ErrorCount + ExceptionCount;

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
    public CycleThreadState State { get; internal set; }

    /// <summary>
    /// 用户自定义的工作数据
    /// </summary>
    public T WorkParameter { get; set; }

    /// <summary>
    /// 清空所有计数器（内部函数，外部调用无效）
    /// </summary>
    public void ClearCount()
    {
        ErrorCount     = 0;
        CycleCount     = 0;
        ExceptionCount = 0;
    }

    /// <summary>
    /// 清空错误计数器（内部函数，外部调用无效）
    /// </summary>
    public void ClearError()
    {
        ErrorCount     = 0;
        ExceptionCount = 0;
    }
}