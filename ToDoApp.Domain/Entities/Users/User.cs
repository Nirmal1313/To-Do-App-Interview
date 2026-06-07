using System;
using System.ComponentModel.DataAnnotations;

namespace ToDoApp.Domain.Entities.User
{
    public class User
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(128)]
        public string FirstName { get; set; } = string.Empty;

        [MaxLength(128)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(10)]
        [MaxLength(128)]
        [DataType(DataType.Password)]
        public string PasswordHash { get; set; } = string.Empty;

        public bool IsDeleted { get; set; }

        public Guid? UpdatedBy { get; set; }

        public DateTime? LastUpdatedDate { get; set; }
    }
}
