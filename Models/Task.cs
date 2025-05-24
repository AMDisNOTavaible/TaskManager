using System;
using System.ComponentModel.DataAnnotations;

namespace TaskManager.Models
{
    public class Task
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public required string Title { get; set; }

        public string? Description { get; set; }

        public DateTime? DueDate { get; set; }

        [Required]
        public required string Status { get; set; } // To Do, In Progress, Done

        public DateTime CreatedAt { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }
    }
}