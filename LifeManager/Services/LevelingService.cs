using LifeManager.Model;

namespace LifeManager.Services;

public class LevelingService
{
    private readonly int[] _thresholds = { 0, 50, 75, 100, 150, 175, 200, 250, 275, 300, 325, 350, 375, 400 };

    public Task<UserLevelModel> CalculateLevelAsync(int totalXp)
    {
        int level = 1;
        int currentLevelXp = totalXp;
        int totalXpForNext = 0;
        int xpForNext = _thresholds[1];
        
        for (int i = 1; i < _thresholds.Length; i++)
        {
            totalXpForNext += _thresholds[i];
      
            if (totalXp >= totalXpForNext)
            {
                level = i + 1;
                currentLevelXp = totalXp - totalXpForNext;
                xpForNext = _thresholds[i + 1];
            }
            else
            {
                break;
            }
        }

        return Task.FromResult(new UserLevelModel(level, currentLevelXp, xpForNext));
    }
}