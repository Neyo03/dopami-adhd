using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using LifeManager.Attributes;

namespace LifeManager.Extensions;

public static class XpExtensions
{
    public static int GetXp(this Enum enumValue)
    {
        var type = enumValue.GetType();
        var memberInfo = type.GetMember(enumValue.ToString()).FirstOrDefault();

        if (memberInfo != null)
        {
            var xpAttribute = memberInfo.GetCustomAttribute<XpValueAttribute>();
            if (xpAttribute != null)
            {
                return xpAttribute.Xp;
            }
        }
        
        return 0;
    }
    
}