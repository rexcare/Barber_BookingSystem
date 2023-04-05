using System.Collections;
using App.Domain.Identity;
using Base.Domain;
using Microsoft.AspNetCore.Mvc;

namespace App.Domain;

public class Vacation: DomainEntityId
{
    public Guid AppUserId { get; set; }
    public AppUser? AppUser { get; set; }
    
    [BindProperty]
    public DateTime DateStart { get; set; }
    [BindProperty]
    public DateTime DateStop { get; set; }
    
    
    [BindProperty]
    public string Dates => DateStart.ToShortDateString() + " - " + DateStop.ToShortDateString();
    
}