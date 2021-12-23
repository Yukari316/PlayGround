using ProtoBuf;
using StackExchange.Redis;

namespace PlayGround
{
    public static class RedisProtoBuf
    {
        public static byte[] ToProtoBytes(this object obj)
        {
            using var stream = new MemoryStream();
            Serializer.Serialize(stream, obj);
            return stream.ToArray();
        }

        public static T ToProtoObject<T>(this RedisValue value)
        {
            return Serializer.Deserialize<T>(new MemoryStream((byte[]) value.Box()));
        }
    }
}
