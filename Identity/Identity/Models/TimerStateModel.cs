namespace Identity.Models
{
    public class TimerStateModel
    {
        public int TaskId { get; set; }
        public string Status { get; set; }
        public long ElapsedTime { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
