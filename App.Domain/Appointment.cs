using App.Domain.Identity;
using Base.Domain;

namespace App.Domain;

public class Appointment: DomainEntityId
{
    public Guid CustomerId { get; set; }
    public Customer? Customer { get; set; }
    
    public Guid ServiceId { get; set; }
    public Service? Service { get; set; }
    
    public Guid AppUserId { get; set; }
    public AppUser? AppUser { get; set; }

    public DateTime StartTime { get; set; } = default!;

}