using System.ComponentModel.DataAnnotations;
using Base.Domain;

namespace App.Domain;

public class Customer: DomainEntityId
{
    [MinLength(3)]
    [MaxLength(128)]
    public string FirstName { get; set; } = default!;

    [MinLength(3)]
    [MaxLength(128)]
    public string LastName { get; set; } = default!;
    
    [MinLength(3)]
    [MaxLength(128)]
    public string Email { get; set; } = default!;
    
    [MinLength(3)]
    [MaxLength(128)]
    public string Phone { get; set; } = default!;
    
    public string FirstLastName => FirstName + " " + LastName;
    public string LastFirstName => LastName + " " + FirstName;
    
    
    public ICollection<Appointment>? Appointments { get; set; }
}