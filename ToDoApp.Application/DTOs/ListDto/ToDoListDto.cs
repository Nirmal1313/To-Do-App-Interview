using System.ComponentModel.DataAnnotations;

namespace ToDoApp.Application.DTOs.ListDto
{
    public class ToDoListDto
    {
        public Guid? Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(20)]
        public string Color { get; set; } = "#000000";

        public int TaskCount { get; set; }
    }
}
