using System.Collections.Generic;
using UnityEngine;

public static class G
{
    public static EnemyGroup enemies;
    public static PartyManager party;
    public static Draggable currentDrag;


    public static AudioSystem audioSystem;

    public static Main main;
    public static ServiceMain ServiceMain;


    public static HUD HUD;
    public static UI UI;

    public static GameFeel feel = new GameFeel();
    public static ScreenFader ScreenFader;
    public static TextPopupManager textPopup;
    public static Hand Hand { get; set; }
}


public static class TextStuff
{
    public static string green = "#65FF5D";
    public static string red = "#F84D47";
    public static string blue => "#0087FF";

    public static Color GetColor(string html)
    {
        if (string.IsNullOrEmpty(html))
            return Color.white;

        if (!html.StartsWith("#"))
            html = "#" + html;

        if (ColorUtility.TryParseHtmlString(html, out var color))
            return color;

        Debug.LogWarning($"[TextStuff] Не удалось распарсить цвет: {html}");
        return Color.magenta;
    }

    public static string ToShortName(this CardType type)
    {
        return type switch
        {
            CardType.Start => "S",
            CardType.Mid => "M",
            CardType.End => "E",
            _ => "?"
        };
    }
}