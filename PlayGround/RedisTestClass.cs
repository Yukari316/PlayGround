using ProtoBuf;

namespace PlayGround
{
    [ProtoContract]
    internal class RedisTestClass
    {
        [ProtoMember(1)]
        public string   RedisKey   { get; set; }
        [ProtoMember(2)]
        public string   RedisValue { get; set; }
        [ProtoMember(3)]
        public DateTime Time       { get; set; }
    }
}
