using Identity.Areas.Identity.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Identity.Models;
public enum TaskPriority
    {
        Low,
        Medium,
        High,
        Critical
    }

    public enum TaskStatus
    {
        ToDo,
        InProgress,
        Testing,
        Done
    }

public class TaskItem
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; }

    public string? Description { get; set; }

    // FK to Project
    [Required]
    public int ProjectId { get; set; }

    [ForeignKey("ProjectId")]
    public Project Project { get; set; }

    // FK to AssignedUser (optional)
    public string? AssignedUserId { get; set; }

    [ForeignKey("AssignedUserId")]
    public ApplicationUser? AssignedUser { get; set; }

    // FK to CreatedBy (always required)
    [Required]
    public string CreatedById { get; set; }

    [ForeignKey("CreatedById")]
    public ApplicationUser? CreatedBy { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal? EstimatedHours { get; set; }

    [Required]
    public TaskPriority Priority { get; set; }

    [Required]
    public TaskStatus Status { get; set; }

    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
}