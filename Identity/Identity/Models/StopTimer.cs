using System.ComponentModel.DataAnnotations;

namespace Identity.Models
{
    public class StopTimer
    {
        public int Id { get; set; }
        public DateTime EndTime { get; set; }
        public int Duration { get; set; }
    }
}
