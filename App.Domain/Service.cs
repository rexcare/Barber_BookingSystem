using Base.Domain;

namespace App.Domain;

public class Service: DomainEntityId
{
    
    public string Name { get; set; } = default!;
    
    public Double Price { get; set; } = default!;
 
    public TimeSpan TimeSpan { get; set; } = default!;
    
    public string Description { get; set; } = default!;
    
    public ICollection<Appointment>? Appointment { get; set; }

}