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
    

public static void OnRunStarted(int startingLevelIndex, int deckSize)
{
    if (!IsInit) return;
    const string eventName = "run_start";
    var customEvent = new CustomEvent(eventName)
    {
        { "starting_level_index", startingLevelIndex },
        { "starting_deck_size", deckSize }
    };
    AnalyticsService.Instance.RecordEvent(customEvent);
}

public static void OnCardDrawn(string cardId, string cardName, string classType, int levelIndex)
{
    if (!IsInit) return;
    const string eventName = "card_drawn";
    var customEvent = new CustomEvent(eventName)
    {
        { "card_id", cardId },
        { "card_name", cardName },
        { "class_type", classType },
        { "level_index", levelIndex }
    };
    AnalyticsService.Instance.RecordEvent(customEvent);
}

public static void OnCardPlayed(string cardId, string cardName, string classType, int levelIndex)
{
    if (!IsInit) return;
    const string eventName = "card_played";
    var customEvent = new CustomEvent(eventName)
    {
        { "card_id", cardId },
        { "card_name", cardName },
        { "class_type", classType },
        { "level_index", levelIndex }
    };
    AnalyticsService.Instance.RecordEvent(customEvent);
}

public static void OnRewardOffered(string rewardId, string rewardType, int slotIndex, int levelIndex)
{
    if (!IsInit) return;
    const string eventName = "reward_offered";
    var customEvent = new CustomEvent(eventName)
    {
        { "reward_id", rewardId },
        { "reward_type", rewardType },
        { "slot_index", slotIndex },
        { "level_index", levelIndex }
    };
    AnalyticsService.Instance.RecordEvent(customEvent);
}

public static void OnRewardPicked(string rewardId, string rewardType, int slotIndex, int levelIndex)
{
    if (!IsInit) return;
    const string eventName = "reward_picked";
    var customEvent = new CustomEvent(eventName)
    {
        { "reward_id", rewardId },
        { "reward_type", rewardType },
        { "slot_index", slotIndex },
        { "level_index", levelIndex }
    };
    AnalyticsService.Instance.RecordEvent(customEvent);
}

public static void OnTutorialShown(string tutorialId)
{
    if (!IsInit) return;
    const string eventName = "tutorial_shown";
    var customEvent = new CustomEvent(eventName)
    {
        { "tutorial_id", tutorialId }
    };
    AnalyticsService.Instance.RecordEvent(customEvent);
}

public static void OnTutorialCompleted(string tutorialId)
{
    if (!IsInit) return;
    const string eventName = "tutorial_completed";
    var customEvent = new CustomEvent(eventName)
    {
        { "tutorial_id", tutorialId }
    };
    AnalyticsService.Instance.RecordEvent(customEvent);
}

public static void OnSettingsChanged(string settingName, float value)
{
    if (!IsInit) return;
    const string eventName = "settings_changed";
    var customEvent = new CustomEvent(eventName)
    {
        { "setting_name", settingName },
        { "value", value }
    };
    AnalyticsService.Instance.RecordEvent(customEvent);
}

public static void OnGameQuit(string place, int levelIndex)
{
    if (!IsInit) return;
    const string eventName = "game_quit";
    var customEvent = new CustomEvent(eventName)
    {
        { "place", place },
        { "level_index", levelIndex }
    };
    AnalyticsService.Instance.RecordEvent(customEvent);
}
}