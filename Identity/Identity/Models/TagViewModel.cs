using System;

namespace Identity.ViewModels
{
    public class TagViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ColorCode { get; set; }
        public DateTime CreatedDate { get; set; }
        public int TaskCount { get; set; }
    }
}