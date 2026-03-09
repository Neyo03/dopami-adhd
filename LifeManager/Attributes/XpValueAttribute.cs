namespace LifeManager.Attributes;

[AttributeUsage(AttributeTargets.Field)]                                                                                      
public class XpValueAttribute(int xp) : Attribute                                                                             
{                                                                                                                             
   public int Xp { get; } = xp;                                                                                              
} 