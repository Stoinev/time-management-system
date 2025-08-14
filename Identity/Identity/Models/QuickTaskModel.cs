namespace Identity.Models
{
    public class QuickTaskModel
    {
        public string Title { get; set; }
        public int? ProjectId { get; set; }
        public TaskStatus Status { get; set; }
    }
}
