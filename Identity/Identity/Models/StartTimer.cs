using System.ComponentModel.DataAnnotations;

namespace Identity.Models
{
    public class StartTimer
    {
        public int TaskId { get; set; }
        public DateTime StartTime { get; set; }
    }
}
