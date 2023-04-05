using System.ComponentModel.DataAnnotations;

namespace WebApp.Models;

public class MultiViewModel
{
    [MinLength(2)]
    [MaxLength(6)]
    [Display(Name = "User name")]
    public string Name { get; set; } = default!;

    public string[] Tags { get; set; } = default!;
}