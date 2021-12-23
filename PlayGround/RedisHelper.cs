using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using StackExchange.Redis;

namespace PlayGround;

[Serializable]
public class RedisObj
{
    public static implicit operator RedisValue(RedisObj input)
    {
        return RedisHelper.SerializeJson(input);
    }
}

public class RedisHelper : IDisposable
{
    private readonly string                                              _connectionString; //连接字符串
    private readonly string                                              _instanceName;     //实例名称
    private readonly int                                                 _defaultDB;        //默认数据库
    private readonly ConcurrentDictionary<string, ConnectionMultiplexer> _connections;

    public RedisHelper(string connectionString,
                       string instanceName,
                       int defaultDB = 0)
    {
        _connectionString = connectionString;
        _instanceName     = instanceName;
        _defaultDB        = defaultDB;
        _connections      = new ConcurrentDictionary<string, ConnectionMultiplexer>();
    }

    /// <summary>
    /// 获取ConnectionMultiplexer
    /// </summary>
    /// <returns></returns>
    private ConnectionMultiplexer GetConnect()
    {
        return _connections.GetOrAdd(_instanceName, p =>
                                                    {
                                                        ConfigurationOptions config =
                                                            ConfigurationOptions.Parse(_connectionString);
                                                        config.KeepAlive          = 10;
                                                        config.AbortOnConnectFail = false;
                                                        ConnectionMultiplexer instance =
                                                            ConnectionMultiplexer.Connect(config);
                                                        instance.ConnectionFailed     += MuxerConnectionFailed;
                                                        instance.ConnectionRestored   += MuxerConnectionRestored;
                                                        instance.ErrorMessage         += MuxerErrorMessage;
                                                        instance.ConfigurationChanged += MuxerConfigurationChanged;
                                                        instance.HashSlotMoved        += MuxerHashSlotMoved;
                                                        instance.InternalError        += MuxerInternalError;
                                                        return instance;
                                                    });
    }

    #region ConnectionMultiplexer事件

    /// <summary>
    /// 配置更改时
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private static void MuxerConfigurationChanged(object sender,
                                                  EndPointEventArgs e)
    {
        Console.WriteLine("Configuration changed: " + e.EndPoint);
    }

    /// <summary>
    /// 发生错误时
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private static void MuxerErrorMessage(object sender,
                                          RedisErrorEventArgs e)
    {
        Console.WriteLine("ErrorMessage: " + e.Message);
    }

    /// <summary>
    /// 重新建立连接之前的错误
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private static void MuxerConnectionRestored(object sender,
                                                ConnectionFailedEventArgs e)
    {
        Console.WriteLine("ConnectionRestored: " + e.EndPoint);
    }

    /// <summary>
    /// 连接失败 ， 如果重新连接成功你将不会收到这个通知
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private static void MuxerConnectionFailed(object sender,
                                              ConnectionFailedEventArgs e)
    {
        Console.WriteLine("重新连接：Endpoint failed: " +
                          e.EndPoint               +
                          ", "                     +
                          e.FailureType            +
                          (e.Exception == null ? "" : ", " + e.Exception.Message));
    }

    /// <summary>
    /// 更改集群
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private static void MuxerHashSlotMoved(object sender,
                                           HashSlotMovedEventArgs e)
    {
        Console.WriteLine("HashSlotMoved:NewEndPoint" + e.NewEndPoint + ", OldEndPoint" + e.OldEndPoint);
    }

    /// <summary>
    /// redis类库错误
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private static void MuxerInternalError(object sender,
                                           InternalErrorEventArgs e)
    {
        Console.WriteLine("InternalError:Message" + e.Exception.Message);
    }

    #endregion

    #region 数据库

    /// <summary>
    /// 获取数据库
    /// </summary>
    public IDatabase GetDatabase()
    {
        return GetConnect().GetDatabase(_defaultDB);
    }

    /// <summary>
    /// 获取数据库
    /// </summary>
    /// <param name="db">默认为0：优先代码的db配置，其次config中的配置</param>
    public IDatabase GetDatabase(int db)
    {
        return GetConnect().GetDatabase(db);
    }

    public IServer GetServer(int endPointsIndex = 0)
    {
        ConfigurationOptions confOption = ConfigurationOptions.Parse(_connectionString);
        return GetConnect().GetServer(confOption.EndPoints[endPointsIndex]);
    }

    public ISubscriber GetSubscriber()
    {
        return GetConnect().GetSubscriber();
    }

    #endregion

    #region 序列化

    /// <summary>
    /// 序列化对象
    /// </summary>
    /// <param name="o"></param>
    /// <returns></returns>
    public static byte[] SerializeJson(object o)
    {
        if (o == null) return null;

        return Encoding.UTF8.GetBytes(JsonSerializer.Serialize(o));
    }

    /// <summary>
    /// 反序列化对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="byteData"></param>
    /// <returns></returns>
    public static T DeserializeJson<T>(byte[] byteData)
    {
        if (byteData == null) return default;

        return JsonSerializer.Deserialize<T>(byteData);
    }

    #endregion

    public void Dispose()
    {
        if (_connections is {Count: > 0})
            foreach (ConnectionMultiplexer item in _connections.Values)
                item.Close();
    }
}