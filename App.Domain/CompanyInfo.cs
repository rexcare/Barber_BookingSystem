﻿using System.ComponentModel.DataAnnotations;
using Base.Domain;

namespace App.Domain;

public class CompanyInfo
{

    public int Id { get; set; }

    [MinLength(1)]
    [MaxLength(128)]
    public string Address { get; set; } = default!;
    
    [MinLength(1)]
    [MaxLength(128)]
    public string Email { get; set; } = default!;
    
    [MinLength(1)]
    [MaxLength(128)]
    public string Phone { get; set; } = default!;

    public string WorkTimeWeek { get; set; } = "8 - 16"!;
    public string WorkTimeWeekEnd { get; set; } = "8 - 15"!;
    public string WorkTimeHolidays { get; set; } = "9 - 14"!;

    [Range(1,9999)]
    public int WorkTimeTemplateInfo { get; set; } = default!;

    public DateTime GenerationDate { get; set; } = default!;
}