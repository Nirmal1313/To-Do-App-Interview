namespace ToDoApp.Domain.Common
{
    public abstract class AuditableEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public Guid? UpdatedBy { get; set; }
        public DateTime? LastUpdatedDate { get; set; }
        public bool IsDeleted { get; set; }
    }

    public abstract class OwnedEntity : AuditableEntity
    {
        public Guid UserId { get; set; }
    }
}
