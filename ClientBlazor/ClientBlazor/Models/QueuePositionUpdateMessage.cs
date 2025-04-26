namespace ClientBlazor.Models
{
    public class QueuePositionUpdateMessage
    {
        public long AcquisitionTime { get; set; }
        public bool HasAcquisitionTime { get; set; }
        public bool HasQueuePosition { get; set; }
        public int ProductId {  get; set; } 
        public uint QueuePosition {  get; set; }
    }
}
