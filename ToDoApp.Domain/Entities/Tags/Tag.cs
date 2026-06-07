using System.ComponentModel.DataAnnotations;
using ToDoApp.Domain.Common;

namespace ToDoApp.Domain.Entities.Tags
{
    public class Tag : OwnedEntity
    {
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(20)]
        public string Color { get; set; } = "#000000";

        public ICollection<TaskTag> TaskTags { get; set; } = new List<TaskTag>();
    }
}
