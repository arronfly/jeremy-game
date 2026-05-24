/// <summary>
/// 难度管理器 - 静态类，存储AI难度参数
/// 控制机器人的准确度、反应时间和伤害倍率
/// </summary>
public static class DifficultyManager
{
    public static float botAccuracy = 0.6f;
    public static float botReactionTime = 0.5f;
    public static float botDamageMultiplier = 1.0f;

    /// <summary>
    /// 根据难度级别设置所有AI参数
    /// Easy: 低准确度、慢反应、低伤害
    /// Normal: 中等参数
    /// Hard: 高准确度、快反应、高伤害
    /// </summary>
    public static void SetDifficulty(string level)
    {
        switch (level.ToLower())
        {
            case "easy":
                botAccuracy = 0.4f;
                botReactionTime = 1.0f;
                botDamageMultiplier = 0.7f;
                break;
            case "normal":
                botAccuracy = 0.6f;
                botReactionTime = 0.5f;
                botDamageMultiplier = 1.0f;
                break;
            case "hard":
                botAccuracy = 0.8f;
                botReactionTime = 0.2f;
                botDamageMultiplier = 1.2f;
                break;
            default:
                botAccuracy = 0.6f;
                botReactionTime = 0.5f;
                botDamageMultiplier = 1.0f;
                break;
        }
    }
}
