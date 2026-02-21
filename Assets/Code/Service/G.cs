using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UIElements;

public static class G
{
    public static EnemyGroup enemies;
    public static PartyManager party;
    public static Draggable currentDrag;

    public static RunState run;

    public static AudioSystem audioSystem;

    public static Main main;
    public static ServiceMain ServiceMain;


    public static HUD HUD;
    public static UI UI;

    public static GameFeel feel = new GameFeel();
    public static ScreenFader ScreenFader;
    public static TextPopupManager textPopup;
    public static Hand Hand { get; set; }
    public static RewardManager Reward { get; set; }
}


public static class TextStuff
{
    public static string green = "#65FF5D";
    public static string red = "#F84D47";
    public static string blue => "#0087FF";

    public static string Damage => "Damage".Color(new Color(0.725f, 0.094f, 0.094f, 1f));
    public static string Hp => "Hp".Color(Color.darkGreen);
    public static string Heal => "Heal".Color(Color.darkGreen);
    public static string Shield => "Shield".Color(Color.darkSlateBlue);
    public static string Tank => "Guardian".Color(Color.darkSlateBlue);

    public static string Bleed => "Bleed".Color(new Color(0.725f, 0.094f, 0.094f, 1f));
    public static string Taunt => "Taunt".Color(Color.darkSlateBlue);
    public static string Vulnerable => "Vulnerable".Color(Color.chocolate);


    public static string GetStatus(StatusEffectType type)
    {
        switch (type)
        {
            case StatusEffectType.None:
                break;
            case StatusEffectType.Bleed:
                return Bleed;
            case StatusEffectType.Vulnerable:
                return Vulnerable;
            case StatusEffectType.Taunt:
                return Taunt;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }

        return null;
    }

    public static string GetClassTypeString(ClassType type)
    {
        switch (type)
        {
            case ClassType.None:
                break;
            case ClassType.Heal:
                return Heal;
            case ClassType.Tank:
                return Tank;

            case ClassType.Damage:
                return Damage;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }

        return null;
    }

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

    private static readonly Regex ColorTagRegex =
        new Regex(@"<color=(?<c>#[0-9a-fA-F]{6,8}|[a-zA-Z]+)>", RegexOptions.Compiled);

    public static string ColoredValue(int value, string coloredLabel)
        => ColoredValue(value.ToString(), coloredLabel);

    public static string ColoredValue(float value, string coloredLabel, string format = "0.#")
        => ColoredValue(value.ToString(format), coloredLabel);

    public static string ColoredValue(string valueText, string coloredLabel)
    {
        if (string.IsNullOrEmpty(coloredLabel))
            return valueText;

        var m = ColorTagRegex.Match(coloredLabel);
        if (!m.Success)
            return $"{valueText} {coloredLabel}";

        var c = m.Groups["c"].Value;
        return $"<color={c}>{valueText}</color> {coloredLabel}";
    }

    public static string ColoredRange(int min, int max, string coloredLabel)
    {
        return TextStuff.ColoredValue($"{min} - {max}", coloredLabel);
    }
}