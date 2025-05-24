using System.ComponentModel.DataAnnotations;

namespace TaskManager.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public required string Name { get; set; }
    }
}