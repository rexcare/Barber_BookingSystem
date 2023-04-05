using System.Runtime.InteropServices.JavaScript;
using App.Domain.Identity;
using Base.Domain;

namespace App.Domain;

public class WorkTime: DomainEntityId
{
    
    public Guid AppUserId { get; set; }
    public AppUser? AppUser { get; set; }
    
    public DateTime Date { get; set; }
    
    public TimeOnly StartTime { get; set; }
    
    public TimeOnly StopTime { get; set; }
    
    public TimeOnly? StartTime1 { get; set; } = default!;
    public TimeOnly? StopTime1 { get; set; } = default!;
    
    public TimeOnly? StartTime2 { get; set; } = default!;
    public TimeOnly? StopTime2 { get; set; } = default!;


    public string DateTime => DataToString();

    public string DataToString()
    {
        var str = Date.ToLongDateString() + " : " + StartTime + "-" + StopTime;
        
        if (StartTime1 != null)
        {
            str += " | " + StartTime1 + "-" + StopTime1;
        }
        if (StartTime2 != null)
        {
            str += " | " + StartTime2 + "-" + StopTime2;
        }

        return str;
    }
    
    
}