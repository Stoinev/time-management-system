namespace Identity.Models
{
    public class Board
    {
        public string Title { get; set; }
        public int ProjectId { get; set; }
        public System.Threading.Tasks.TaskStatus Status { get; set; }
    }
}

