﻿using System.ComponentModel.DataAnnotations;

namespace Time_Management_System.Models
{
    public class TaskTag
    {
        public int TaskId { get; set; }
        public int TagId { get; set; }

        public TaskItem Task { get; set; }
        public Tag Tag { get; set; }
    }
}
