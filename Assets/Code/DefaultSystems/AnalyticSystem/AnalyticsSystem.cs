using System.Threading.Tasks;
using Unity.Services.Analytics;
using Unity.Services.Core;
using UnityEngine.UnityConsent;

public static class AnalyticsSystem
{
    private static bool IsInit = false;

    public static async Task Init()
    {
#if !UNITY_EDITOR

        await UnityServices.InitializeAsync();

        var state = new ConsentState
        {
            AnalyticsIntent = ConsentStatus.Granted,
            AdsIntent = ConsentStatus.Denied
        };

        EndUserConsent.SetConsentState(state);
        IsInit = true;

#else
        await UnityServices.InitializeAsync();
#endif
    }

    public static void OnLevelChanged(int levelIndex, string levelName)
    {
        if (!IsInit) return;
        const string eventName = "level_changed";
        var customEvent = new CustomEvent(eventName)
        {
            { "level_index", levelIndex },
            { "level_name", levelName }
        };
        AnalyticsService.Instance.RecordEvent(customEvent);
    }

    public static void OnGameEnded(string result, int totalLevelsPassed)
    {
        if (!IsInit) return;
        const string eventName = "game_end";
        var customEvent = new CustomEvent(eventName)
        {
            { "result", result },
            { "total_levels_passed", totalLevelsPassed }
        };
        AnalyticsService.Instance.RecordEvent(customEvent);
    }
}