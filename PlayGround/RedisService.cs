using StackExchange.Redis;

namespace PlayGround;

public static class RedisExtend
{
    public static List<string> AsStringList(this RedisValue[] input)
    {
        List<string> ret = new();
        foreach (RedisValue obj in input)
            try
            {
                ret.Add(obj.ToString());
            }
            catch
            {
                ret.Add(default);
            }

        return ret;
    }

    public static List<T> ToObject<T>(this RedisValue[] input)
    {
        List<T> ret = new List<T>();
        foreach (RedisValue obj in input)
            try
            {
                ret.Add(RedisHelper.DeserializeJson<T>(obj));
            }
            catch
            {
                ret.Add(default);
            }

        return ret;
    }

    public static T ToObject<T>(this RedisValue input)
    {
        try
        {
            return RedisHelper.DeserializeJson<T>(input);
        }
        catch
        {
            return default;
        }
    }
}