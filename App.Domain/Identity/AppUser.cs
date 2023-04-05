using System.Collections;
using System.ComponentModel.DataAnnotations;
using Base.Domain.Identity;

namespace App.Domain.Identity;

public class AppUser : BaseUser
{
    [MinLength(3)]
    [MaxLength(128)]
    public string FirstName { get; set; } = default!;

    [MinLength(3)]
    [MaxLength(128)]
    public string LastName { get; set; } = default!;
    

    public string? Role { get; set; }

    public bool? IsSelected { get; set; } = default!;

    public Guid WorkTimeTemplateId { get; set; } = default!;
    public WorkTimeTemplate? WorkTimeTemplate { get; set; } = default!;

    public ICollection<Vacation>? Vacations { get; set; }
    
    public ICollection<WorkTime>? WorkTimes { get; set; }
    
    public ICollection<ReservedTime>? ReservedTime { get; set; }

    public ICollection<RefreshToken>? RefreshTokens { get; set; }

    public string FirstLastName => FirstName + " " + LastName;
    public string LastFirstName => LastName + " " + FirstName;
}
