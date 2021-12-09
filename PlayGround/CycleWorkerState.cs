namespace PlayGround;

public enum CycleWorkerState
{
    /// <summary>
    /// 执行中
    /// </summary>
    Running,
    /// <summary>
    /// 中断
    /// </summary>
    Interrupted,
    /// <summary>
    /// 停止
    /// </summary>
    Stopped,
    /// <summary>
    /// 已销毁
    /// </summary>
    Disposed
}