using Identity.Models;

namespace Identity.Models
{
    public class TaskTag
    {
        public int TaskId { get; set; }
        public TaskItem TaskItem { get; set; }

        public int TagId { get; set; }
        public Tag Tag { get; set; }
    }
}