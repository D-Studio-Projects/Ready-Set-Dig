using UnityEngine;

public static class LauncherForceCalculator
{
    #region Public Methods

    public static float Calculate(
        float _minigameResult,
        LauncherData _launcherData,
        int _level)
    {
        float normalizedResult = Mathf.Clamp01(_minigameResult);

        if (_launcherData == null || !_launcherData.HasValidConfiguration())
            return normalizedResult;

        // FinalForce = MinigameResult * (BaseForce + ForceGainPerLevel * Level).
        float forceMultiplier = _launcherData.GetForceMultiplier(_level);
        return Mathf.Clamp01(normalizedResult * forceMultiplier);
    }

    #endregion
}
