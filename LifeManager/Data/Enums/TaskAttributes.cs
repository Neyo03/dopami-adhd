using System.ComponentModel.DataAnnotations;
using LifeManager.Attributes;

namespace LifeManager.Data.Enums;

public enum TaskDuration
{
    NotSpecified = 0,
    
    [Display(Name = "2 à 5 min")]
    [XpValue(2)]
    TwoToFiveMins = 1,
    
    [Display(Name = "10 min")]
    [XpValue(5)]
    TenMins = 2,
    
    [Display(Name = "20+ min")]
    [XpValue(10)]
    TwentyPlusMins = 3
}

public enum TaskEnergy
{
    NotSpecified = 0,
    
    [Display(Name = "Basse")]
    [XpValue(2)]
    Low = 1,
    
    [Display(Name = "Moyenne")]
    [XpValue(5)]
    Medium = 2,
    
    [Display(Name = "Haute")]
    [XpValue(10)]
    High = 3
}

public enum TaskImpact
{
    NotSpecified = 0,
    
    [Display(Name = "Bloquant")]
    [XpValue(2)]
    Blocking = 1,
    
    [Display(Name = "Visible rapide")]
    [XpValue(5)]
    QuickVisible = 2,
    
    [Display(Name = "Soulage l’anxiété")]
    [XpValue(10)]
    AnxietyRelief = 3
}