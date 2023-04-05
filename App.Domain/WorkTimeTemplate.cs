using System.ComponentModel.DataAnnotations;
using App.Domain.Identity;
using Base.Domain;

namespace App.Domain;

public class WorkTimeTemplate: DomainEntityId
{
    public TimeOnly StartTime { get; set; } = TimeOnly.Parse("08:00");
    public TimeOnly StopTime { get; set; } = TimeOnly.Parse("16:00");

    public TimeOnly? StartTime1 { get; set; } = default!;
    public TimeOnly? StopTime1 { get; set; } = default!;
    
    public TimeOnly? StartTime2 { get; set; } = default!;
    public TimeOnly? StopTime2 { get; set; } = default!;


    public ICollection<AppUser>? AppUsers { get; set; } = default!;
    
    public string Times => TimesToString();


    public string TimesToString()
    {
        string times;

        times = StartTime + "-" + StopTime;
        if (StartTime1 != null)
        {
            times += " | " + StartTime1 + "-" + StopTime1;
        }
        if (StartTime2 != null)
        {
            times += " | " + StartTime2 + "-" + StopTime2;
        }
        return times;
    }
}