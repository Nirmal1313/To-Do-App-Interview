using System.ComponentModel.DataAnnotations;

namespace ToDoApp.Application.DTOs.TagDto
{
    public class TagDto
    {
        public Guid? Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(20)]
        public string Color { get; set; } = "#000000";
    }
}
