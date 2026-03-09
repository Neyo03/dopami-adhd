namespace LifeManager.Model;

// Data Transfer Object for the UI
public record UserLevelModel(int Level, int CurrentLevelXp, int XpRequiredForNextLevel, string CurrentLevelName);