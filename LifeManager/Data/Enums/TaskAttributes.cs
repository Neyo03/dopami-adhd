using System.ComponentModel.DataAnnotations;

namespace LifeManager.Data.Enums;

public enum TaskDuration
{
    NotSpecified = 0,
    [Display(Name = "2 à 5 min")]
    TwoToFiveMins = 1,
    
    [Display(Name = "10 min")]
    TenMins = 2,
    
    [Display(Name = "20+ min")]
    TwentyPlusMins = 3
}

public enum TaskEnergy
{
    NotSpecified = 0,
    
    [Display(Name = "Basse")]
    Low = 1,
    [Display(Name = "Moyenne")]
    Medium = 2,
    [Display(Name = "Haute")]
    High = 3
}

public enum TaskImpact
{
    NotSpecified = 0,
    [Display(Name = "Bloquant")]
    Blocking = 1,
    [Display(Name = "Visible rapide")]
    QuickVisible = 2,
    [Display(Name = "Soulage l’anxiété")]
    AnxietyRelief = 3
}