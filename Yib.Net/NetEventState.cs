namespace Net
{
    public class NetEventState
    {
        public NetEventState(NetEventType type, NetEventResult result)
        {
            EventType = type;
            EventResult = result;
        }

        public NetEventType EventType { get; set; }
        public NetEventResult EventResult { get; set; }
    }
}
