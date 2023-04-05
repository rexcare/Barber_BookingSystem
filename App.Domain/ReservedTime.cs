using App.Domain.Identity;
using Base.Domain;

namespace App.Domain;

public class ReservedTime: DomainEntityId
{
    public DateOnly Date { get; set; }
    
    public TimeOnly StartTime { get; set; }
    
    public TimeOnly StopTime { get; set; }
    
    public Guid AppUserId { get; set; }
    public AppUser? AppUser { get; set; }
    
}