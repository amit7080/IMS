namespace IMS.Data.Interface
{
    public interface IAuditableEntity
    {
        DateTime? CreationDate { get; set; }
        string? CreatedBy { get; set; }
        DateTime? ModificationDate { get; set; }
        string? ModifiedBy { get; set; }
        bool IsDeleted { get; set; }
        bool IsActive { get; set; }
    }
}
