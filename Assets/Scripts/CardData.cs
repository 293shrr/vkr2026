using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NewCard", menuName = "Cards/Card Data")]
public class CardData : ScriptableObject
{
    [TextArea(3, 6)]
    public string description;

    [Header("Presentation")]
    public string cardTitle;

    [TextArea(1, 3)]
    public string bottomInfo;

    [Header("Visuals")]
    public Sprite illustration;

    public ChoiceData leftChoice;
    public ChoiceData rightChoice;

    [Header("Flags")]
    public bool isReturnCard;
    public bool isSpecialEvent;
}

[Serializable]
public class ChoiceData
{
    public string choiceText;

    [Header("Stat changes")]
    public int healthDelta;
    public int staminaDelta;
    public int resourcesDelta;
    public int moraleDelta;

    [Header("Raid")]
    public int lootDelta;
    public int timeDelta = 1;

    [Header("Flow")]
    public bool startRaid;
    public bool endRaid;

    [Header("Shop upgrades")]
    public int maxHealthDelta;
    public int maxStaminaDelta;

    [Header("Meta progress")]
    public int progressDelta;

    [Header("Final")]
    [TextArea(2, 5)]
    public string finalMessage;
}
