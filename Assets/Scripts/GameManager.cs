using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Current Stats")]
    public int health = 5;
    public int stamina = 5;
    public int resources = 3;
    public int morale = 4;

    [Header("Max Stats")]
    public int maxHealth = 10;
    public int maxStamina = 10;

    [Header("Surface Data")]
    public int surfaceData = 0;
    [SerializeField] private int requiredSurfaceData = 10;
    [SerializeField] private CardData finalCard = null;

    private bool finalCardQueued = false;
    private CardData activeFinalCard = null;

    [Header("Raid")]
    public int currentLoot = 0;
    public int raidTime = 0;
    [SerializeField] private int nightStartsAt = 10;
    [SerializeField] private int lootReturnThreshold = 3;
    public GamePhase currentPhase = GamePhase.Base;

    [Header("Raid Prompt Logic")]
    [SerializeField] private int cardsBeforeForcedReturnPrompt = 2;
    private int cardsSinceLootThreshold = 0;
    private bool forceReturnCard = false;

    [Header("Special Event Chances")]
    [SerializeField, Range(0, 100)] private int specialDayEventChance = 10;
    [SerializeField, Range(0, 100)] private int specialNightEventChance = 20;

    [Header("Card Sets")]
    [SerializeField] private CardData[] baseCards = null;
    [SerializeField] private CardData[] raidDayCards = null;
    [SerializeField] private CardData[] raidNightCards = null;

    [Header("UI")]
    [SerializeField] private TMP_Text healthText = null;
    [SerializeField] private TMP_Text staminaText = null;
    [SerializeField] private TMP_Text resourcesText = null;
    [SerializeField] private TMP_Text moraleText = null;
    [SerializeField] private TMP_Text raidStatusText = null;
    [SerializeField] private TMP_Text cardText = null;
    [SerializeField] private TMP_Text cardInfoText = null;
    [SerializeField] private TMP_Text leftChoiceText = null;
    [SerializeField] private TMP_Text rightChoiceText = null;
    [SerializeField] private TMP_Text phaseText = null;

    [Header("Visuals")]
    [SerializeField] private Image backgroundImage = null;
    [SerializeField] private Image topBarImage = null;
    [SerializeField] private Image cardDescriptionPanelImage = null;
    [SerializeField] private Image cardImage = null;
    [SerializeField] private Image cardIllustrationImage = null;
    [SerializeField] private Sprite baseBackground = null;
    [SerializeField] private Sprite raidDayBackground = null;
    [SerializeField] private Sprite raidNightBackground = null;
    [SerializeField] private Sprite defaultBaseCardIllustration = null;
    [SerializeField] private Sprite defaultRaidDayCardIllustration = null;
    [SerializeField] private Sprite defaultRaidNightCardIllustration = null;
    [SerializeField] private Sprite menuLogo = null;
    [SerializeField] private Sprite menuBackground = null;

    [Header("Game Over UI")]
    [SerializeField] private GameObject gameOverPanel = null;
    [SerializeField] private TMP_Text gameOverReasonText = null;

    private int currentCardIndex = 0;
    private CardData lastShownCard = null;
    private bool isGameOver = false;
    private Color defaultBackgroundColor = new Color(0.07058824f, 0.08627451f, 0.1254902f, 1f);
    private readonly Color metallicPanelColor = new Color(0.17f, 0.19f, 0.20f, 1f);
    private readonly Color metallicTopBarColor = new Color(0.075f, 0.082f, 0.088f, 0.98f);
    private readonly Color metallicTextColor = new Color(0.86f, 0.83f, 0.75f, 1f);
    private Image leftChoicePanel;
    private Image rightChoicePanel;
    private Image hudStatusPanel;
    private GamePhase lastVisualPhase;
    private readonly List<CardData> recentShownCards = new List<CardData>();
    private int baseCardsSinceRaidOffer = 0;
    private GameObject mainMenuPanel;
    private GameObject tutorialPanel;
    private GameObject gameplayMenuButtonObject;
    private Sprite cardBackSprite;
    private Sprite menuBackgroundSprite;
    private bool hasGameplayStarted = false;

    private int initialHealth;
    private int initialStamina;
    private int initialResources;
    private int initialMorale;
    private int initialMaxHealth;
    private int initialMaxStamina;
    private int initialSurfaceData;
    private int initialCurrentLoot;
    private int initialRaidTime;
    private GamePhase initialCurrentPhase;

    private const string SaveExistsKey = "RaiderGame_SaveExists";
    private const string SaveHealthKey = "RaiderGame_Health";
    private const string SaveStaminaKey = "RaiderGame_Stamina";
    private const string SaveResourcesKey = "RaiderGame_Resources";
    private const string SaveMoraleKey = "RaiderGame_Morale";
    private const string SaveMaxHealthKey = "RaiderGame_MaxHealth";
    private const string SaveMaxStaminaKey = "RaiderGame_MaxStamina";
    private const string SaveSurfaceDataKey = "RaiderGame_SurfaceData";
    private const string SaveCurrentLootKey = "RaiderGame_CurrentLoot";
    private const string SaveRaidTimeKey = "RaiderGame_RaidTime";
    private const string SaveCurrentPhaseKey = "RaiderGame_CurrentPhase";
    private const string SaveCurrentCardIndexKey = "RaiderGame_CurrentCardIndex";
    private const string SaveFinalCardQueuedKey = "RaiderGame_FinalCardQueued";
    private const string SaveActiveFinalCardKey = "RaiderGame_ActiveFinalCard";
    private const string SaveCardsSinceLootThresholdKey = "RaiderGame_CardsSinceLootThreshold";
    private const string SaveForceReturnCardKey = "RaiderGame_ForceReturnCard";

    private void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;

        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        CacheInitialState();
        BindVisualReferences();
    }

    private void Start()
    {
        SetGameOverPanelVisible(false);

        UpdateUI();
        ShowCurrentCard();
        RefreshHudState();
        ShowMainMenu();
    }

    [ContextMenu("Apply Runtime Visual Layout")]
    public void ApplyRuntimeVisualLayout()
    {
        BindVisualReferences();
        UpdateUI();
        ShowCurrentCard();
        RefreshHudState(true);
    }

    private void CacheInitialState()
    {
        initialHealth = health;
        initialStamina = stamina;
        initialResources = resources;
        initialMorale = morale;
        initialMaxHealth = maxHealth;
        initialMaxStamina = maxStamina;
        initialSurfaceData = surfaceData;
        initialCurrentLoot = currentLoot;
        initialRaidTime = raidTime;
        initialCurrentPhase = currentPhase;
    }

    private void StartGameplay()
    {
        hasGameplayStarted = true;
        isGameOver = false;

        SetGameOverPanelVisible(false);

        SetGameplayMenuButtonVisible(true);
        UpdateUI();
        ShowCurrentCard();
        RefreshHudState(true);
        SaveGame();
    }

    private void StartNewGameFromMenu()
    {
        DeleteSave();
        ResetRuntimeState();
        HideMainMenu();
        ShowTutorialPage();
    }

    private void ContinueGameFromMenu()
    {
        if (!LoadGame())
            return;

        HideMainMenu();
        StartGameplay();
    }

    private void ResetRuntimeState()
    {
        health = initialHealth;
        stamina = initialStamina;
        resources = initialResources;
        morale = initialMorale;
        maxHealth = initialMaxHealth;
        maxStamina = initialMaxStamina;
        surfaceData = initialSurfaceData;
        currentLoot = initialCurrentLoot;
        raidTime = initialRaidTime;
        currentPhase = initialCurrentPhase;
        currentCardIndex = 0;
        lastShownCard = null;
        recentShownCards.Clear();
        baseCardsSinceRaidOffer = 0;
        finalCardQueued = false;
        activeFinalCard = null;
        cardsSinceLootThreshold = 0;
        forceReturnCard = false;
        isGameOver = false;

        ClampStats();
    }

    private void ShowMainMenu()
    {
        if (mainMenuPanel != null)
            return;

        SetGameplayMenuButtonVisible(false);

        GameObject canvasObject = FindSceneObject("Canvas");
        if (canvasObject == null)
            return;

        mainMenuPanel = new GameObject("MainMenuPanel", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
        mainMenuPanel.transform.SetParent(canvasObject.transform, false);
        mainMenuPanel.transform.SetAsLastSibling();

        RectTransform panelTransform = mainMenuPanel.GetComponent<RectTransform>();
        panelTransform.anchorMin = Vector2.zero;
        panelTransform.anchorMax = Vector2.one;
        panelTransform.offsetMin = Vector2.zero;
        panelTransform.offsetMax = Vector2.zero;

        Image panelImage = mainMenuPanel.GetComponent<Image>();
        panelImage.sprite = menuBackground != null ? menuBackground : GetOrCreateMenuBackgroundSprite();
        panelImage.type = Image.Type.Simple;
        panelImage.preserveAspect = false;
        panelImage.color = Color.white;
        panelImage.raycastTarget = true;

        CanvasGroup canvasGroup = mainMenuPanel.GetComponent<CanvasGroup>();
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;

        float aspect = Screen.width > 0 ? Screen.height / (float)Screen.width : 1.78f;
        bool compactMenu = aspect < 1.55f;
        bool portraitMenu = aspect >= 1.55f;

        CreateMenuLogo(portraitMenu ? 0.76f : 0.74f, portraitMenu ? new Vector2(2100f, 780f) : new Vector2(2600f, 980f));

        bool hasSave = HasSave();
        Vector2 buttonSize = portraitMenu ? new Vector2(760f, 132f) : compactMenu ? new Vector2(760f, 128f) : new Vector2(820f, 142f);
        CreateMenuButton("Продолжить", portraitMenu ? 0.49f : compactMenu ? 0.45f : 0.47f, buttonSize, hasSave, ContinueGameFromMenu);
        CreateMenuButton("Новая игра", portraitMenu ? 0.38f : compactMenu ? 0.32f : 0.36f, buttonSize, true, StartNewGameFromMenu);
        CreateMenuButton("Выход", portraitMenu ? 0.27f : compactMenu ? 0.19f : 0.25f, buttonSize, true, ExitGame);
    }

    private Sprite GetOrCreateMenuBackgroundSprite()
    {
        if (menuBackgroundSprite != null)
            return menuBackgroundSprite;

        const int width = 512;
        const int height = 768;
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        Color topColor = new Color(0.030f, 0.034f, 0.038f, 1f);
        Color bottomColor = new Color(0.010f, 0.012f, 0.014f, 1f);
        Color rustGlow = new Color(0.18f, 0.105f, 0.045f, 1f);
        Color coldGlow = new Color(0.045f, 0.075f, 0.085f, 1f);

        for (int y = 0; y < height; y++)
        {
            float v = y / (float)(height - 1);

            for (int x = 0; x < width; x++)
            {
                float u = x / (float)(width - 1);
                float noise = Mathf.PerlinNoise(u * 7.5f, v * 8.5f);
                float fineNoise = Mathf.PerlinNoise(u * 28f + 11f, v * 24f + 5f);
                Color color = Color.Lerp(bottomColor, topColor, v);

                float leftGlow = Mathf.Clamp01(1f - Mathf.Abs(u - 0.22f) * 3.2f) * Mathf.Clamp01(1f - Mathf.Abs(v - 0.38f) * 2.2f);
                float rightGlow = Mathf.Clamp01(1f - Mathf.Abs(u - 0.78f) * 3.0f) * Mathf.Clamp01(1f - Mathf.Abs(v - 0.58f) * 2.4f);
                color = Color.Lerp(color, rustGlow, leftGlow * 0.38f);
                color = Color.Lerp(color, coldGlow, rightGlow * 0.32f);

                float beam = Mathf.Clamp01(1f - Mathf.Abs((u - 0.50f) + (v - 0.58f) * 0.22f) * 5.8f);
                color += new Color(0.08f, 0.07f, 0.045f, 0f) * beam * Mathf.Clamp01(v * 1.2f) * 0.28f;

                if ((x % 86) < 2 && v > 0.10f && v < 0.88f)
                    color += new Color(0.045f, 0.047f, 0.043f, 0f);

                if ((y % 118) < 2 && v > 0.16f && v < 0.82f)
                    color += new Color(0.025f, 0.025f, 0.022f, 0f);

                color += new Color(noise, noise, noise, 0f) * 0.030f;
                color -= new Color(fineNoise, fineNoise, fineNoise, 0f) * 0.018f;

                float dx = Mathf.Abs(u - 0.5f);
                float dy = Mathf.Abs(v - 0.52f);
                float vignette = Mathf.Clamp01((dx * dx * 1.75f + dy * dy * 1.35f) * 2.25f);
                color = Color.Lerp(color, Color.black, vignette * 0.62f);

                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();
        menuBackgroundSprite = Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), 100f);
        menuBackgroundSprite.name = "GeneratedMenuBackground";

        return menuBackgroundSprite;
    }

    private void CreateMenuLogo(float normalizedY, Vector2 size)
    {
        GameObject logoObject = new GameObject("MenuLogo", typeof(RectTransform), typeof(Image));
        logoObject.transform.SetParent(mainMenuPanel.transform, false);

        Image logoImage = logoObject.GetComponent<Image>();
        logoImage.sprite = menuLogo;
        logoImage.color = menuLogo != null ? Color.white : metallicTextColor;
        logoImage.preserveAspect = true;
        logoImage.raycastTarget = false;

        RectTransform logoTransform = logoImage.rectTransform;
        logoTransform.anchorMin = new Vector2(0.5f, normalizedY);
        logoTransform.anchorMax = new Vector2(0.5f, normalizedY);
        logoTransform.pivot = new Vector2(0.5f, 0.5f);
        logoTransform.anchoredPosition = Vector2.zero;
        logoTransform.sizeDelta = size;

        if (menuLogo == null)
        {
            Destroy(logoObject);
            TMP_Text fallbackText = CreateMenuText("MenuTitleFallback", "RAIDER GAME", Vector2.zero, size, 58f, 80f, FontStyles.Bold);
            RectTransform fallbackTransform = fallbackText.rectTransform;
            fallbackTransform.anchorMin = new Vector2(0.5f, normalizedY);
            fallbackTransform.anchorMax = new Vector2(0.5f, normalizedY);
            fallbackTransform.anchoredPosition = Vector2.zero;
            return;
        }
    }

    private TMP_Text CreateMenuText(string objectName, string label, Vector2 anchoredPosition, Vector2 size, float minSize, float maxSize, FontStyles fontStyle)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(mainMenuPanel.transform, false);

        TMP_Text text = textObject.GetComponent<TMP_Text>();
        text.text = label;
        text.color = metallicTextColor;
        text.alignment = TextAlignmentOptions.Center;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.enableAutoSizing = true;
        text.fontSizeMin = minSize;
        text.fontSizeMax = maxSize;
        text.fontStyle = fontStyle;
        text.raycastTarget = false;

        RectTransform rectTransform = text.rectTransform;
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = size;

        return text;
    }

    private Button CreateMenuButton(string label, Vector2 anchoredPosition, bool isInteractable, UnityAction onClick)
    {
        GameObject buttonObject = new GameObject(label + "Button", typeof(RectTransform), typeof(Image), typeof(Outline), typeof(Button));
        buttonObject.transform.SetParent(mainMenuPanel.transform, false);

        RectTransform buttonTransform = buttonObject.GetComponent<RectTransform>();
        buttonTransform.anchorMin = new Vector2(0.5f, 0.5f);
        buttonTransform.anchorMax = new Vector2(0.5f, 0.5f);
        buttonTransform.pivot = new Vector2(0.5f, 0.5f);
        buttonTransform.anchoredPosition = anchoredPosition;
        buttonTransform.sizeDelta = new Vector2(700f, 126f);

        Image buttonImage = buttonObject.GetComponent<Image>();
        buttonImage.color = isInteractable
            ? new Color(0.070f, 0.080f, 0.078f, 0.98f)
            : new Color(0.040f, 0.046f, 0.046f, 0.90f);

        Outline outline = buttonObject.GetComponent<Outline>();
        outline.effectColor = isInteractable
            ? new Color(0.72f, 0.62f, 0.36f, 0.82f)
            : new Color(0.28f, 0.26f, 0.20f, 0.45f);
        outline.effectDistance = new Vector2(4f, -4f);
        outline.useGraphicAlpha = false;

        Button button = buttonObject.GetComponent<Button>();
        button.interactable = isInteractable;
        button.onClick.AddListener(onClick);

        ColorBlock colors = button.colors;
        colors.normalColor = buttonImage.color;
        colors.highlightedColor = new Color(0.18f, 0.17f, 0.11f, 1f);
        colors.pressedColor = new Color(0.055f, 0.062f, 0.060f, 1f);
        colors.disabledColor = new Color(0.040f, 0.046f, 0.046f, 0.88f);
        button.colors = colors;

        GameObject textObject = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(buttonObject.transform, false);

        TMP_Text buttonText = textObject.GetComponent<TMP_Text>();
        buttonText.text = label;
        buttonText.color = isInteractable ? metallicTextColor : new Color(0.42f, 0.42f, 0.40f, 1f);
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.enableAutoSizing = true;
        buttonText.fontSizeMin = 52f;
        buttonText.fontSizeMax = 74f;
        buttonText.fontStyle = FontStyles.Bold;
        buttonText.raycastTarget = false;

        RectTransform textTransform = buttonText.rectTransform;
        textTransform.anchorMin = new Vector2(0.07f, 0.10f);
        textTransform.anchorMax = new Vector2(0.93f, 0.90f);
        textTransform.offsetMin = Vector2.zero;
        textTransform.offsetMax = Vector2.zero;

        return button;
    }

    private Button CreateMenuButton(string label, float normalizedY, Vector2 size, bool isInteractable, UnityAction onClick)
    {
        Button button = CreateMenuButton(label, Vector2.zero, isInteractable, onClick);
        RectTransform buttonTransform = button.GetComponent<RectTransform>();
        buttonTransform.anchorMin = new Vector2(0.5f, normalizedY);
        buttonTransform.anchorMax = new Vector2(0.5f, normalizedY);
        buttonTransform.anchoredPosition = Vector2.zero;
        buttonTransform.sizeDelta = size;

        return button;
    }

    private void ShowTutorialPage()
    {
        if (tutorialPanel != null)
            return;

        GameObject canvasObject = FindSceneObject("Canvas");
        if (canvasObject == null)
            return;

        tutorialPanel = new GameObject("TutorialPanel", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
        SetGameplayMenuButtonVisible(false);
        tutorialPanel.transform.SetParent(canvasObject.transform, false);
        tutorialPanel.transform.SetAsLastSibling();

        RectTransform panelTransform = tutorialPanel.GetComponent<RectTransform>();
        panelTransform.anchorMin = Vector2.zero;
        panelTransform.anchorMax = Vector2.one;
        panelTransform.offsetMin = Vector2.zero;
        panelTransform.offsetMax = Vector2.zero;

        Image panelImage = tutorialPanel.GetComponent<Image>();
        panelImage.color = new Color(0.018f, 0.021f, 0.024f, 1f);
        panelImage.raycastTarget = true;

        CanvasGroup canvasGroup = tutorialPanel.GetComponent<CanvasGroup>();
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;

        Transform previousParent = mainMenuPanel != null ? mainMenuPanel.transform : null;
        mainMenuPanel = tutorialPanel;

        CreateMenuText("TutorialTitle", "Как играть", new Vector2(0f, 265f), new Vector2(760f, 82f), 48f, 64f, FontStyles.Bold);
        CreateMenuText("TutorialBody",
            "Тяни карту пальцем влево или вправо.\n\nСлева и справа появится выбранное действие. Отпусти карту за край, чтобы подтвердить выбор.\n\nСледи за HP, выносливостью, ресурсами и репутацией сверху.",
            new Vector2(0f, 45f), new Vector2(800f, 310f), 30f, 42f, FontStyles.Bold);
        CreateMenuButton("Начать", new Vector2(0f, -260f), true, CloseTutorialAndStart);

        mainMenuPanel = previousParent != null ? previousParent.gameObject : null;
    }

    private void CloseTutorialAndStart()
    {
        if (tutorialPanel != null)
        {
            Destroy(tutorialPanel);
            tutorialPanel = null;
        }

        StartGameplay();
    }

    private void ExitGame()
    {
        SaveGame();
        Application.Quit();
    }

    private void ReturnToMainMenu()
    {
        SaveGame();
        HideChoicePreview();
        hasGameplayStarted = false;
        SetGameplayMenuButtonVisible(false);
        ShowMainMenu();
    }

    private void HideMainMenu()
    {
        if (mainMenuPanel == null)
            return;

        Destroy(mainMenuPanel);
        mainMenuPanel = null;
    }

    private bool HasSave()
    {
        return PlayerPrefs.GetInt(SaveExistsKey, 0) == 1;
    }

    private void SaveGame()
    {
        if (!hasGameplayStarted || isGameOver)
            return;

        PlayerPrefs.SetInt(SaveExistsKey, 1);
        PlayerPrefs.SetInt(SaveHealthKey, health);
        PlayerPrefs.SetInt(SaveStaminaKey, stamina);
        PlayerPrefs.SetInt(SaveResourcesKey, resources);
        PlayerPrefs.SetInt(SaveMoraleKey, morale);
        PlayerPrefs.SetInt(SaveMaxHealthKey, maxHealth);
        PlayerPrefs.SetInt(SaveMaxStaminaKey, maxStamina);
        PlayerPrefs.SetInt(SaveSurfaceDataKey, surfaceData);
        PlayerPrefs.SetInt(SaveCurrentLootKey, currentLoot);
        PlayerPrefs.SetInt(SaveRaidTimeKey, raidTime);
        PlayerPrefs.SetInt(SaveCurrentPhaseKey, (int)currentPhase);
        PlayerPrefs.SetInt(SaveCurrentCardIndexKey, currentCardIndex);
        PlayerPrefs.SetInt(SaveFinalCardQueuedKey, finalCardQueued ? 1 : 0);
        PlayerPrefs.SetInt(SaveActiveFinalCardKey, activeFinalCard != null ? 1 : 0);
        PlayerPrefs.SetInt(SaveCardsSinceLootThresholdKey, cardsSinceLootThreshold);
        PlayerPrefs.SetInt(SaveForceReturnCardKey, forceReturnCard ? 1 : 0);
        PlayerPrefs.Save();
    }

    private bool LoadGame()
    {
        if (!HasSave())
            return false;

        health = PlayerPrefs.GetInt(SaveHealthKey, initialHealth);
        stamina = PlayerPrefs.GetInt(SaveStaminaKey, initialStamina);
        resources = PlayerPrefs.GetInt(SaveResourcesKey, initialResources);
        morale = PlayerPrefs.GetInt(SaveMoraleKey, initialMorale);
        maxHealth = PlayerPrefs.GetInt(SaveMaxHealthKey, initialMaxHealth);
        maxStamina = PlayerPrefs.GetInt(SaveMaxStaminaKey, initialMaxStamina);
        surfaceData = PlayerPrefs.GetInt(SaveSurfaceDataKey, initialSurfaceData);
        currentLoot = PlayerPrefs.GetInt(SaveCurrentLootKey, initialCurrentLoot);
        raidTime = PlayerPrefs.GetInt(SaveRaidTimeKey, initialRaidTime);

        int savedPhase = PlayerPrefs.GetInt(SaveCurrentPhaseKey, (int)initialCurrentPhase);
        currentPhase = System.Enum.IsDefined(typeof(GamePhase), savedPhase)
            ? (GamePhase)savedPhase
            : initialCurrentPhase;

        currentCardIndex = PlayerPrefs.GetInt(SaveCurrentCardIndexKey, 0);
        finalCardQueued = PlayerPrefs.GetInt(SaveFinalCardQueuedKey, 0) == 1;
        activeFinalCard = PlayerPrefs.GetInt(SaveActiveFinalCardKey, 0) == 1 ? finalCard : null;
        cardsSinceLootThreshold = PlayerPrefs.GetInt(SaveCardsSinceLootThresholdKey, 0);
        forceReturnCard = PlayerPrefs.GetInt(SaveForceReturnCardKey, 0) == 1;
        isGameOver = false;

        ClampStats();
        lastShownCard = GetCurrentCard();

        return true;
    }

    private void DeleteSave()
    {
        PlayerPrefs.DeleteKey(SaveExistsKey);
        PlayerPrefs.DeleteKey(SaveHealthKey);
        PlayerPrefs.DeleteKey(SaveStaminaKey);
        PlayerPrefs.DeleteKey(SaveResourcesKey);
        PlayerPrefs.DeleteKey(SaveMoraleKey);
        PlayerPrefs.DeleteKey(SaveMaxHealthKey);
        PlayerPrefs.DeleteKey(SaveMaxStaminaKey);
        PlayerPrefs.DeleteKey(SaveSurfaceDataKey);
        PlayerPrefs.DeleteKey(SaveCurrentLootKey);
        PlayerPrefs.DeleteKey(SaveRaidTimeKey);
        PlayerPrefs.DeleteKey(SaveCurrentPhaseKey);
        PlayerPrefs.DeleteKey(SaveCurrentCardIndexKey);
        PlayerPrefs.DeleteKey(SaveFinalCardQueuedKey);
        PlayerPrefs.DeleteKey(SaveActiveFinalCardKey);
        PlayerPrefs.DeleteKey(SaveCardsSinceLootThresholdKey);
        PlayerPrefs.DeleteKey(SaveForceReturnCardKey);
        PlayerPrefs.Save();
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause)
            SaveGame();
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }

    private void LateUpdate()
    {
        RefreshHudState();
    }

    private void RefreshHudState()
    {
        RefreshHudState(false);
    }

    private void RefreshHudState(bool forceVisualRefresh)
    {
        CardData currentCard = GetCurrentCard();

        if (raidStatusText != null)
        {
            raidStatusText.text = GetStatusLabel();
            raidStatusText.gameObject.SetActive(true);
        }

        if (phaseText != null)
        {
            phaseText.text = GetHudContextLabel(currentCard);
            phaseText.gameObject.SetActive(true);
        }

        UpdateBackgroundVisual(forceVisualRefresh);
    }

    public void SwipeLeft()
    {
        if (!hasGameplayStarted) return;
        if (isGameOver) return;

        CardData currentCard = GetCurrentCard();
        if (currentCard == null) return;

        ChoiceData choice = currentCard.leftChoice;
        if (choice == null)
        {
            Debug.LogWarning($"Card '{currentCard.name}' is missing a left choice.");
            return;
        }

        if (!CanApplyChoice(choice))
        {
            ShowCannotAffordMessage();
            return;
        }

        ApplyChoice(choice);
        ProcessChoiceFlow(choice, currentCard);
    }

    public void SwipeRight()
    {
        if (!hasGameplayStarted) return;
        if (isGameOver) return;

        CardData currentCard = GetCurrentCard();
        if (currentCard == null) return;

        ChoiceData choice = currentCard.rightChoice;
        if (choice == null)
        {
            Debug.LogWarning($"Card '{currentCard.name}' is missing a right choice.");
            return;
        }

        if (!CanApplyChoice(choice))
        {
            ShowCannotAffordMessage();
            return;
        }

        ApplyChoice(choice);
        ProcessChoiceFlow(choice, currentCard);
    }

    private bool CanApplyChoice(ChoiceData choice)
    {
        if (choice == null)
            return false;

        int futureResources = resources + choice.resourcesDelta;
        return futureResources >= 0;
    }

    private void ShowCannotAffordMessage()
    {
        if (cardText != null)
            cardText.text = "Недостаточно ресурсов для этого действия.";
    }

    private void ApplyChoice(ChoiceData choice)
    {
        if (choice == null)
            return;

        int resourceDelta = choice.resourcesDelta;
        if (currentPhase == GamePhase.Base && resourceDelta > 0)
            resourceDelta = Mathf.Min(resourceDelta, 1);

        health += choice.healthDelta;
        stamina += choice.staminaDelta;
        resources += resourceDelta;
        morale += choice.moraleDelta;

        surfaceData += choice.progressDelta;
        surfaceData = Mathf.Clamp(surfaceData, 0, 999);

        maxHealth += choice.maxHealthDelta;
        maxStamina += choice.maxStaminaDelta;

        maxHealth = Mathf.Max(1, maxHealth);
        maxStamina = Mathf.Max(1, maxStamina);

        int finalLoot = choice.lootDelta;
        if (currentPhase == GamePhase.RaidDay && finalLoot > 0)
            finalLoot = Mathf.Min(finalLoot, 1);

        if (currentPhase == GamePhase.RaidNight && finalLoot > 0)
            finalLoot = Mathf.Min(finalLoot * 2, 2);

        currentLoot += finalLoot;

        if (currentPhase == GamePhase.RaidDay || currentPhase == GamePhase.RaidNight)
        {
            raidTime += Mathf.Max(0, choice.timeDelta);
        }

        ClampStats();
        currentLoot = Mathf.Clamp(currentLoot, 0, 999);
    }

    private void ProcessChoiceFlow(ChoiceData choice, CardData sourceCard)
    {
        if (choice == null)
            return;

        string loseReason = GetLoseReason();
        if (!string.IsNullOrEmpty(loseReason))
        {
            TriggerGameOver(loseReason);
            return;
        }

        if (sourceCard == activeFinalCard)
        {
            string message = string.IsNullOrWhiteSpace(choice.finalMessage)
                ? "Экспедиция завершена. Судьба Сперанцы определена."
                : choice.finalMessage;

            TriggerGameOver(message);
            return;
        }

        if (choice.startRaid)
        {
            StartRaid();
        }
        else if (choice.endRaid)
        {
            if (morale <= 0)
            {
                TriggerGameOver("Тебя не пустили обратно в Сперанцу. Тебе больше не доверяют.");
                return;
            }

            EndRaid();
        }
        else
        {
            UpdateRaidPhaseByTime();
            UpdateForcedReturnLogic(sourceCard);
            QueueFinalCardIfNeeded();
            NextCard();
        }

        UpdateUI();
        ShowCurrentCard();
        RefreshHudState();

        loseReason = GetLoseReason();
        if (!string.IsNullOrEmpty(loseReason))
        {
            TriggerGameOver(loseReason);
            DeleteSave();
        }
        else
        {
            SaveGame();
        }
    }

    private void StartRaid()
    {
        activeFinalCard = null;

        currentPhase = GamePhase.RaidDay;
        currentCardIndex = 0;
        currentLoot = 0;
        raidTime = 0;
        cardsSinceLootThreshold = 0;
        forceReturnCard = false;
        recentShownCards.Clear();
        baseCardsSinceRaidOffer = 0;

        NextCard();
    }

    private void EndRaid()
    {
        resources += currentLoot;

        currentLoot = 0;
        raidTime = 0;
        currentPhase = GamePhase.Base;
        currentCardIndex = 0;
        cardsSinceLootThreshold = 0;
        forceReturnCard = false;
        recentShownCards.Clear();
        baseCardsSinceRaidOffer = 0;

        ClampStats();
        QueueFinalCardIfNeeded();
    }

    private void QueueFinalCardIfNeeded()
    {
        if (!finalCardQueued &&
            surfaceData >= requiredSurfaceData &&
            finalCard != null &&
            currentPhase == GamePhase.Base)
        {
            finalCardQueued = true;
            activeFinalCard = finalCard;
        }
    }

    private void UpdateRaidPhaseByTime()
    {
        if (currentPhase == GamePhase.RaidDay && raidTime >= nightStartsAt)
        {
            currentPhase = GamePhase.RaidNight;
            currentCardIndex = 0;
            recentShownCards.Clear();
        }
    }

    private void UpdateForcedReturnLogic(CardData sourceCard)
    {
        if (currentPhase != GamePhase.RaidDay && currentPhase != GamePhase.RaidNight)
            return;

        if (sourceCard != null && sourceCard.isReturnCard)
            return;

        if (currentLoot >= lootReturnThreshold)
        {
            cardsSinceLootThreshold++;

            if (cardsSinceLootThreshold >= cardsBeforeForcedReturnPrompt)
            {
                forceReturnCard = true;
                cardsSinceLootThreshold = 0;
            }
        }
        else
        {
            cardsSinceLootThreshold = 0;
        }
    }

    private void NextCard()
    {
        if (currentPhase == GamePhase.Base && activeFinalCard != null)
        {
            return;
        }

        CardData[] currentSet = GetCurrentCardSet();

        if (currentSet == null || currentSet.Length == 0)
            return;

        if ((currentPhase == GamePhase.RaidDay || currentPhase == GamePhase.RaidNight) && forceReturnCard)
        {
            int returnIndex = GetReturnCardIndex(currentSet);
            if (returnIndex != -1)
            {
                currentCardIndex = returnIndex;
                forceReturnCard = false;
                return;
            }
        }

        if (currentPhase == GamePhase.Base && baseCardsSinceRaidOffer >= 2)
        {
            CardData startRaidCard = GetRandomStartRaidCard(currentSet);
            int startRaidIndex = FindCardIndexInCurrentSet(startRaidCard, currentSet);

            if (startRaidIndex != -1)
            {
                currentCardIndex = startRaidIndex;
                baseCardsSinceRaidOffer = 0;
                RememberShownCard(startRaidCard);
                return;
            }
        }

        CardData[] possibleCards = GetEligibleCards(currentSet, lastShownCard);

        if (possibleCards == null || possibleCards.Length == 0)
            return;

        int specialChance = 0;

        if (currentPhase == GamePhase.RaidDay)
            specialChance = specialDayEventChance;

        if (currentPhase == GamePhase.RaidNight)
            specialChance = specialNightEventChance;

        if ((currentPhase == GamePhase.RaidDay || currentPhase == GamePhase.RaidNight) &&
            Random.Range(0, 100) < specialChance)
        {
            CardData specialCard = GetRandomSpecialCard(possibleCards);

            if (specialCard != null)
            {
                int specialIndex = FindCardIndexInCurrentSet(specialCard, currentSet);

                if (specialIndex != -1)
                {
                    currentCardIndex = specialIndex;
                    RememberShownCard(specialCard);
                    return;
                }
            }
        }

        CardData normalCard = GetRandomNormalCard(possibleCards);

        if (normalCard != null)
        {
            int normalIndex = FindCardIndexInCurrentSet(normalCard, currentSet);

            if (normalIndex != -1)
            {
                currentCardIndex = normalIndex;
                RememberShownCard(normalCard);
                UpdateBaseRaidOfferCounter(normalCard);
            }
        }
    }

    private CardData[] GetEligibleCards(CardData[] source, CardData excludedCard = null)
    {
        if (source == null || source.Length == 0)
            return null;

        List<CardData> result = new List<CardData>();
        List<CardData> fallback = new List<CardData>();

        for (int i = 0; i < source.Length; i++)
        {
            if (source[i] == null) continue;
            if (source[i].isReturnCard) continue;

            fallback.Add(source[i]);

            if (source[i] == excludedCard) continue;
            if (recentShownCards.Contains(source[i])) continue;

            result.Add(source[i]);
        }

        if (result.Count == 0)
            return fallback.ToArray();

        return result.ToArray();
    }

    private CardData GetRandomSpecialCard(CardData[] cards)
    {
        List<CardData> specialCards = new List<CardData>();

        for (int i = 0; i < cards.Length; i++)
        {
            if (cards[i] != null && cards[i].isSpecialEvent)
                specialCards.Add(cards[i]);
        }

        if (specialCards.Count == 0)
            return null;

        return specialCards[Random.Range(0, specialCards.Count)];
    }

    private CardData GetRandomNormalCard(CardData[] cards)
    {
        List<CardData> normalCards = new List<CardData>();

        for (int i = 0; i < cards.Length; i++)
        {
            if (cards[i] != null && !cards[i].isSpecialEvent)
                normalCards.Add(cards[i]);
        }

        if (normalCards.Count == 0)
            return cards[Random.Range(0, cards.Length)];

        return normalCards[Random.Range(0, normalCards.Count)];
    }

    private CardData GetRandomStartRaidCard(CardData[] cards)
    {
        List<CardData> startRaidCards = new List<CardData>();
        List<CardData> fallback = new List<CardData>();

        for (int i = 0; i < cards.Length; i++)
        {
            CardData card = cards[i];
            if (card == null || !IsStartRaidCard(card))
                continue;

            fallback.Add(card);

            if (!recentShownCards.Contains(card) && card != lastShownCard)
                startRaidCards.Add(card);
        }

        if (startRaidCards.Count == 0)
            startRaidCards = fallback;

        if (startRaidCards.Count == 0)
            return null;

        return startRaidCards[Random.Range(0, startRaidCards.Count)];
    }

    private bool IsStartRaidCard(CardData card)
    {
        if (card == null)
            return false;

        return (card.leftChoice != null && card.leftChoice.startRaid) ||
               (card.rightChoice != null && card.rightChoice.startRaid);
    }

    private void RememberShownCard(CardData card)
    {
        if (card == null || card.isReturnCard)
            return;

        recentShownCards.Remove(card);
        recentShownCards.Add(card);

        int maxRecentCards = currentPhase == GamePhase.Base ? 4 : 12;
        while (recentShownCards.Count > maxRecentCards)
            recentShownCards.RemoveAt(0);
    }

    private void UpdateBaseRaidOfferCounter(CardData card)
    {
        if (currentPhase != GamePhase.Base)
            return;

        if (IsStartRaidCard(card))
            baseCardsSinceRaidOffer = 0;
        else
            baseCardsSinceRaidOffer++;
    }

    private int GetReturnCardIndex(CardData[] cards)
    {
        for (int i = 0; i < cards.Length; i++)
        {
            if (cards[i] != null && cards[i].isReturnCard)
                return i;
        }

        return -1;
    }

    private int FindCardIndexInCurrentSet(CardData target, CardData[] cards)
    {
        if (target == null || cards == null)
            return -1;

        for (int i = 0; i < cards.Length; i++)
        {
            if (cards[i] == target)
                return i;
        }

        return -1;
    }

    private CardData GetCurrentCard()
    {
        if (currentPhase == GamePhase.Base && activeFinalCard != null)
            return activeFinalCard;

        CardData[] currentSet = GetCurrentCardSet();

        if (currentSet == null || currentSet.Length == 0)
            return null;

        if (currentCardIndex < 0 || currentCardIndex >= currentSet.Length)
            currentCardIndex = 0;

        return currentSet[currentCardIndex];
    }

    private CardData[] GetCurrentCardSet()
    {
        switch (currentPhase)
        {
            case GamePhase.Base:
                return baseCards;

            case GamePhase.RaidDay:
                return raidDayCards;

            case GamePhase.RaidNight:
                return raidNightCards;

            default:
                return baseCards;
        }
    }

    private void ShowCurrentCard()
    {
        if (isGameOver) return;

        CardData currentCard = GetCurrentCard();

        if (currentCard == null)
        {
            if (cardText != null) cardText.text = "Нет карточек для текущей фазы.";
            if (cardInfoText != null) cardInfoText.text = "";
            if (leftChoiceText != null) leftChoiceText.text = "";
            if (rightChoiceText != null) rightChoiceText.text = "";
            if (raidStatusText != null) raidStatusText.text = "";

            UpdateCardIllustration(null);
            return;
        }

        if (cardText != null)
            cardText.text = currentCard.description;

        if (cardInfoText != null)
            cardInfoText.text = "";

        UpdateCardIllustration(GetCardIllustration(currentCard));

        if (leftChoiceText != null)
            leftChoiceText.text = currentCard.leftChoice != null ? currentCard.leftChoice.choiceText : "";

        if (rightChoiceText != null)
            rightChoiceText.text = currentCard.rightChoice != null ? currentCard.rightChoice.choiceText : "";

        lastShownCard = currentCard;

        HideChoicePreview();
    }

    private void UpdateCardIllustration(Sprite illustration)
    {
        if (cardIllustrationImage == null)
            return;

        bool hasIllustration = illustration != null;

        cardIllustrationImage.sprite = illustration;
        cardIllustrationImage.gameObject.SetActive(hasIllustration);
    }

    public void PreviewChoice(float swipeProgress)
    {
        float clampedProgress = Mathf.Clamp(swipeProgress, -1f, 1f);
        float alpha = Mathf.Clamp01((Mathf.Abs(clampedProgress) - 0.04f) / 0.38f);

        SetChoicePreview(leftChoiceText, leftChoicePanel, clampedProgress < -0.05f ? alpha : 0f);
        SetChoicePreview(rightChoiceText, rightChoicePanel, clampedProgress > 0.05f ? alpha : 0f);
    }

    public void HideChoicePreview()
    {
        SetChoicePreview(leftChoiceText, leftChoicePanel, 0f);
        SetChoicePreview(rightChoiceText, rightChoicePanel, 0f);
    }

    private void SetChoicePreview(TMP_Text text, Image panel, float alpha)
    {
        if (panel != null)
        {
            Color panelColor = panel.color;
            panelColor.a = 0.92f * alpha;
            panel.color = panelColor;
            panel.gameObject.SetActive(alpha > 0.01f);
        }

        if (text != null)
        {
            Color color = text.color;
            color.a = alpha;
            text.color = color;
            text.gameObject.SetActive(alpha > 0.01f);
        }
    }

    private Sprite GetCardIllustration(CardData card)
    {
        if (card != null && card.illustration != null)
            return card.illustration;

        switch (currentPhase)
        {
            case GamePhase.Base:
                return defaultBaseCardIllustration;

            case GamePhase.RaidDay:
                return defaultRaidDayCardIllustration;

            case GamePhase.RaidNight:
                return defaultRaidNightCardIllustration;

            default:
                return null;
        }
    }

    private void UpdateBackgroundVisual(bool force = false)
    {
        if (backgroundImage == null)
            return;

        Sprite background = GetPhaseBackground();

        if (!force && backgroundImage.sprite == background && lastVisualPhase == currentPhase)
            return;

        backgroundImage.sprite = background;
        backgroundImage.color = background != null ? Color.white : defaultBackgroundColor;
        backgroundImage.gameObject.SetActive(background != null || backgroundImage.color.a > 0f);
        lastVisualPhase = currentPhase;
    }

    private Sprite GetPhaseBackground()
    {
        switch (currentPhase)
        {
            case GamePhase.Base:
                return baseBackground;

            case GamePhase.RaidDay:
                return raidDayBackground;

            case GamePhase.RaidNight:
                return raidNightBackground;

            default:
                return null;
        }
    }

    private void BindVisualReferences()
    {
        if (backgroundImage == null)
        {
            GameObject backgroundObject = FindSceneObject("BackGround");
            if (backgroundObject != null)
                backgroundImage = backgroundObject.GetComponent<Image>();
        }

        if (backgroundImage != null)
            defaultBackgroundColor = backgroundImage.color;

        if (topBarImage == null)
        {
            GameObject topBarObject = FindSceneObject("TopBar");
            if (topBarObject != null)
                topBarImage = topBarObject.GetComponent<Image>();
        }

        GameObject cardObject = FindSceneObject("Card");
        if (cardImage == null && cardObject != null)
            cardImage = cardObject.GetComponent<Image>();

        if (cardDescriptionPanelImage == null)
        {
            GameObject descriptionPanelObject = FindSceneObject("CardDescriptionPanel");
            if (descriptionPanelObject != null)
                cardDescriptionPanelImage = descriptionPanelObject.GetComponent<Image>();
        }

        if (cardDescriptionPanelImage == null)
            cardDescriptionPanelImage = CreateCardDescriptionPanel();

        if (cardIllustrationImage == null)
            cardIllustrationImage = FindCardIllustrationImage();

        if (cardInfoText == null)
            cardInfoText = FindOrCreateCardInfoText();

        ConfigureReignsLikeCardLayout();
    }

    private Image FindCardIllustrationImage()
    {
        GameObject existingObject = FindSceneObject("CardIllustrationImage");
        if (existingObject != null)
            return existingObject.GetComponent<Image>();

        GameObject cardObject = FindSceneObject("Card");
        if (cardObject == null)
            return null;

        RectTransform cardTransform = cardObject.GetComponent<RectTransform>();
        if (cardTransform == null)
            return null;

        GameObject illustrationObject = new GameObject("CardIllustrationImage", typeof(RectTransform), typeof(Image));
        illustrationObject.transform.SetParent(cardTransform, false);
        illustrationObject.transform.SetAsFirstSibling();

        RectTransform illustrationTransform = illustrationObject.GetComponent<RectTransform>();
        illustrationTransform.anchorMin = Vector2.zero;
        illustrationTransform.anchorMax = Vector2.one;
        illustrationTransform.offsetMin = new Vector2(28f, 28f);
        illustrationTransform.offsetMax = new Vector2(-28f, -28f);

        Image image = illustrationObject.GetComponent<Image>();
        image.preserveAspect = true;
        image.raycastTarget = false;
        image.color = Color.white;
        image.gameObject.SetActive(false);

        return image;
    }

    private TMP_Text FindOrCreateCardInfoText()
    {
        GameObject existingObject = FindSceneObject("CardInfoText");
        if (existingObject != null)
            return existingObject.GetComponent<TMP_Text>();

        GameObject cardObject = FindSceneObject("Card");
        if (cardObject == null)
            return null;

        GameObject infoObject = new GameObject("CardInfoText", typeof(RectTransform), typeof(TextMeshProUGUI));
        infoObject.transform.SetParent(cardObject.transform, false);

        TMP_Text infoText = infoObject.GetComponent<TMP_Text>();
        infoText.text = "";
        infoText.color = Color.white;
        infoText.alignment = TextAlignmentOptions.Center;
        infoText.textWrappingMode = TextWrappingModes.Normal;
        infoText.enableAutoSizing = true;
        infoText.fontSizeMin = 22f;
        infoText.fontSizeMax = 30f;
        infoText.raycastTarget = false;

        return infoText;
    }

    private void ConfigureReignsLikeCardLayout()
    {
        ConfigureTopBarLayout();
        ConfigureCardFrame();
        ConfigureCardStoryText();
        ConfigureTextRect(cardInfoText, new Vector2(0.08f, 0.05f), new Vector2(0.92f, 0.16f), 22f, 30f, TextAlignmentOptions.Center);

        if (cardInfoText != null)
            cardInfoText.gameObject.SetActive(false);

        leftChoicePanel = FindOrCreateChoicePanel("LeftChoicePanel", true);
        rightChoicePanel = FindOrCreateChoicePanel("RightChoicePanel", false);

        AttachChoiceTextToPanel(leftChoiceText, leftChoicePanel);
        AttachChoiceTextToPanel(rightChoiceText, rightChoicePanel);
        CreateGameplayMenuButton();

        if (cardIllustrationImage != null)
        {
            RectTransform rectTransform = cardIllustrationImage.rectTransform;
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = new Vector2(28f, 28f);
            rectTransform.offsetMax = new Vector2(-28f, -28f);

            cardIllustrationImage.preserveAspect = true;
            cardIllustrationImage.raycastTarget = false;
            cardIllustrationImage.transform.SetAsFirstSibling();
        }

        HideChoicePreview();
    }

    private void CreateGameplayMenuButton()
    {
        if (gameplayMenuButtonObject != null)
            return;

        GameObject canvasObject = FindSceneObject("Canvas");
        if (canvasObject == null)
            return;

        gameplayMenuButtonObject = new GameObject("GameplayMenuButton", typeof(RectTransform), typeof(Image), typeof(Outline), typeof(Button));
        gameplayMenuButtonObject.transform.SetParent(canvasObject.transform, false);

        RectTransform buttonTransform = gameplayMenuButtonObject.GetComponent<RectTransform>();
        buttonTransform.anchorMin = new Vector2(1f, 1f);
        buttonTransform.anchorMax = new Vector2(1f, 1f);
        buttonTransform.pivot = new Vector2(1f, 1f);
        buttonTransform.anchoredPosition = new Vector2(-26f, -212f);
        buttonTransform.sizeDelta = new Vector2(170f, 64f);

        Image buttonImage = gameplayMenuButtonObject.GetComponent<Image>();
        buttonImage.color = new Color(0.045f, 0.050f, 0.052f, 0.84f);

        Outline outline = gameplayMenuButtonObject.GetComponent<Outline>();
        outline.effectColor = new Color(0.72f, 0.62f, 0.36f, 0.60f);
        outline.effectDistance = new Vector2(2f, -2f);
        outline.useGraphicAlpha = false;

        Button button = gameplayMenuButtonObject.GetComponent<Button>();
        button.onClick.AddListener(ReturnToMainMenu);

        GameObject textObject = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(gameplayMenuButtonObject.transform, false);

        TMP_Text buttonText = textObject.GetComponent<TMP_Text>();
        buttonText.text = "Меню";
        buttonText.color = metallicTextColor;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.enableAutoSizing = true;
        buttonText.fontSizeMin = 24f;
        buttonText.fontSizeMax = 36f;
        buttonText.fontStyle = FontStyles.Bold;
        buttonText.raycastTarget = false;

        RectTransform textTransform = buttonText.rectTransform;
        textTransform.anchorMin = new Vector2(0.08f, 0.12f);
        textTransform.anchorMax = new Vector2(0.92f, 0.88f);
        textTransform.offsetMin = Vector2.zero;
        textTransform.offsetMax = Vector2.zero;

        SetGameplayMenuButtonVisible(false);
    }

    private void SetGameplayMenuButtonVisible(bool isVisible)
    {
        if (gameplayMenuButtonObject != null)
            gameplayMenuButtonObject.SetActive(isVisible);
    }

    private void ConfigureTopBarLayout()
    {
        if (topBarImage == null)
            return;

        RectTransform rectTransform = topBarImage.rectTransform;
        rectTransform.anchorMin = new Vector2(0f, 1f);
        rectTransform.anchorMax = new Vector2(1f, 1f);
        rectTransform.pivot = new Vector2(0.5f, 1f);
        rectTransform.anchoredPosition = new Vector2(0f, 2f);
        rectTransform.sizeDelta = new Vector2(0f, 190f);

        topBarImage.color = metallicTopBarColor;

        ConfigureTopBarSlot(healthText, "HealthIcon", 0f);
        ConfigureTopBarSlot(staminaText, "StaminaIcon", 0.25f);
        ConfigureTopBarSlot(resourcesText, "ResourcesIcon", 0.50f);
        ConfigureTopBarSlot(moraleText, "ReputationIcon", 0.75f);
    }

    private void ConfigureTopBarSlot(TMP_Text text, string iconName, float slotStart)
    {
        float slotEnd = slotStart + 0.25f;
        ConfigureTopBarIconRect(iconName, new Vector2(slotStart + 0.018f, 0.24f), new Vector2(slotStart + 0.078f, 0.76f));
        ConfigureTopBarTextRect(text, new Vector2(slotStart + 0.102f, 0.08f), new Vector2(slotEnd - 0.012f, 0.92f), 36f, 50f, TextAlignmentOptions.MidlineLeft);
    }

    private void ConfigureTopBarIconRect(string iconName, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject iconObject = FindSceneObject(iconName);
        if (iconObject == null)
            return;

        if (topBarImage != null && iconObject.transform.parent != topBarImage.transform)
            iconObject.transform.SetParent(topBarImage.transform, false);

        RectTransform rectTransform = iconObject.GetComponent<RectTransform>();
        if (rectTransform == null)
            return;

        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        rectTransform.pivot = new Vector2(0.5f, 0.5f);

        Image iconImage = iconObject.GetComponent<Image>();
        if (iconImage != null)
        {
            iconImage.color = new Color(0.86f, 0.83f, 0.75f, 0.55f);
            iconImage.raycastTarget = false;
            iconImage.preserveAspect = true;
        }
    }

    private void ConfigureCardFrame()
    {
        if (cardImage == null)
            return;

        RectTransform rectTransform = cardImage.rectTransform;
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = new Vector2(0f, -145f);
        rectTransform.sizeDelta = new Vector2(760f, 960f);

        cardImage.sprite = GetOrCreateCardBackSprite();
        cardImage.type = Image.Type.Simple;
        cardImage.preserveAspect = false;
        cardImage.color = Color.white;
        cardImage.raycastTarget = true;
    }

    private Sprite GetOrCreateCardBackSprite()
    {
        if (cardBackSprite != null)
            return cardBackSprite;

        const int width = 256;
        const int height = 320;
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        Color32 baseColor = new Color32(29, 33, 32, 255);
        Color32 darkerColor = new Color32(20, 24, 24, 255);
        Color32 lineColor = new Color32(47, 53, 50, 255);
        Color32 goldColor = new Color32(116, 102, 67, 255);
        Color32 brightGoldColor = new Color32(146, 126, 78, 255);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color32 color = baseColor;

                if (((x + y) % 34) < 2 || ((x - y + height) % 42) < 2)
                    color = lineColor;

                if (x < 10 || x >= width - 10 || y < 10 || y >= height - 10)
                    color = goldColor;

                if (x < 4 || x >= width - 4 || y < 4 || y >= height - 4)
                    color = brightGoldColor;

                if (x > 26 && x < width - 26 && y > 26 && y < height - 26 &&
                    (x < 31 || x > width - 31 || y < 31 || y > height - 31))
                    color = goldColor;

                int centerX = Mathf.Abs(x - width / 2);
                int centerY = Mathf.Abs(y - height / 2);
                if (centerX + centerY > 34 && centerX + centerY < 41)
                    color = brightGoldColor;
                else if (centerX + centerY < 28)
                    color = darkerColor;

                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();
        cardBackSprite = Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), 100f);
        cardBackSprite.name = "GeneratedCardBack";

        return cardBackSprite;
    }

    private void ConfigureCardStoryText()
    {
        if (cardText == null)
            return;

        if (cardDescriptionPanelImage != null && cardText.transform.parent != cardDescriptionPanelImage.transform)
            cardText.transform.SetParent(cardDescriptionPanelImage.transform, false);

        ConfigureDescriptionPanel();
        ConfigureTextRect(cardText, new Vector2(0.05f, 0.10f), new Vector2(0.95f, 0.90f), 36f, 50f, TextAlignmentOptions.Center);
        cardText.color = metallicTextColor;
        cardText.fontStyle = FontStyles.Bold;
        cardText.gameObject.SetActive(true);

        ConfigurePhaseLabelUnderCard();
    }

    private Image CreateCardDescriptionPanel()
    {
        GameObject canvasObject = FindSceneObject("Canvas");
        if (canvasObject == null)
            return null;

        GameObject panelObject = new GameObject("CardDescriptionPanel", typeof(RectTransform), typeof(Image));
        panelObject.transform.SetParent(canvasObject.transform, false);

        Image image = panelObject.GetComponent<Image>();
        image.color = new Color(0.075f, 0.082f, 0.088f, 0.92f);
        image.raycastTarget = false;

        return image;
    }

    private void ConfigureDescriptionPanel()
    {
        if (cardDescriptionPanelImage == null)
            return;

        RectTransform rectTransform = cardDescriptionPanelImage.rectTransform;
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = new Vector2(0f, 485f);
        rectTransform.sizeDelta = new Vector2(760f, 240f);

        cardDescriptionPanelImage.color = new Color(0.075f, 0.082f, 0.088f, 0.92f);
        cardDescriptionPanelImage.raycastTarget = false;
    }

    private void ConfigurePhaseLabelUnderCard()
    {
        hudStatusPanel = FindOrCreateHudStatusPanel();

        if (hudStatusPanel != null)
        {
            RectTransform panelTransform = hudStatusPanel.rectTransform;
            panelTransform.anchorMin = new Vector2(0.5f, 0.5f);
            panelTransform.anchorMax = new Vector2(0.5f, 0.5f);
            panelTransform.pivot = new Vector2(0.5f, 0.5f);
            panelTransform.anchoredPosition = new Vector2(0f, -730f);
            panelTransform.sizeDelta = new Vector2(760f, 150f);

            hudStatusPanel.color = new Color(0.045f, 0.050f, 0.052f, 0.78f);
            hudStatusPanel.raycastTarget = false;
            hudStatusPanel.transform.SetAsLastSibling();
        }

        ConfigureFloatingText(phaseText, new Vector2(0f, -700f), new Vector2(720f, 78f), 38f, 52f, TextAlignmentOptions.Center);
        ConfigureFloatingText(raidStatusText, new Vector2(0f, -766f), new Vector2(720f, 64f), 28f, 38f, TextAlignmentOptions.Center);
    }

    private Image FindOrCreateHudStatusPanel()
    {
        GameObject existingObject = FindSceneObject("HudStatusPanel");
        if (existingObject != null)
            return existingObject.GetComponent<Image>();

        GameObject canvasObject = FindSceneObject("Canvas");
        if (canvasObject == null)
            return null;

        GameObject panelObject = new GameObject("HudStatusPanel", typeof(RectTransform), typeof(Image));
        panelObject.transform.SetParent(canvasObject.transform, false);

        Image image = panelObject.GetComponent<Image>();
        image.raycastTarget = false;

        return image;
    }

    private void ConfigureFloatingText(TMP_Text text, Vector2 anchoredPosition, Vector2 size, float minSize, float maxSize, TextAlignmentOptions alignment)
    {
        if (text == null)
            return;

        GameObject canvasObject = FindSceneObject("Canvas");
        if (canvasObject != null && text.transform.parent != canvasObject.transform)
            text.transform.SetParent(canvasObject.transform, false);

        RectTransform rectTransform = text.rectTransform;
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = size;

        text.alignment = alignment;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.enableAutoSizing = true;
        text.fontSizeMin = minSize;
        text.fontSizeMax = maxSize;
        text.color = metallicTextColor;
        text.fontStyle = FontStyles.Bold;
        text.raycastTarget = false;
        text.transform.SetAsLastSibling();
    }

    private Image FindOrCreateChoicePanel(string panelName, bool isLeft)
    {
        GameObject existingObject = FindSceneObject(panelName);
        Image panel = existingObject != null ? existingObject.GetComponent<Image>() : null;

        if (panel == null)
        {
            GameObject cardObject = FindSceneObject("Card");
            if (cardObject == null)
                return null;

            GameObject panelObject = new GameObject(panelName, typeof(RectTransform), typeof(Image));
            panelObject.transform.SetParent(cardObject.transform, false);
            panel = panelObject.GetComponent<Image>();
        }

        RectTransform rectTransform = panel.rectTransform;
        rectTransform.anchorMin = isLeft ? new Vector2(0f, 1f) : new Vector2(1f, 1f);
        rectTransform.anchorMax = isLeft ? new Vector2(0f, 1f) : new Vector2(1f, 1f);
        rectTransform.pivot = isLeft ? new Vector2(0f, 1f) : new Vector2(1f, 1f);
        rectTransform.anchoredPosition = isLeft ? new Vector2(34f, -118f) : new Vector2(-34f, -118f);
        rectTransform.sizeDelta = new Vector2(440f, 160f);
        rectTransform.localRotation = Quaternion.identity;

        panel.color = new Color(0.070f, 0.074f, 0.068f, 0f);
        panel.raycastTarget = false;
        panel.gameObject.SetActive(false);
        panel.transform.SetAsLastSibling();

        return panel;
    }

    private void AttachChoiceTextToPanel(TMP_Text text, Image panel)
    {
        if (text == null || panel == null)
            return;

        text.transform.SetParent(panel.transform, false);
        ConfigureTextRect(text, new Vector2(0.06f, 0.08f), new Vector2(0.94f, 0.92f), 32f, 46f, TextAlignmentOptions.Center);
        text.color = new Color(0.98f, 0.91f, 0.70f, 0f);
        text.fontStyle = FontStyles.Bold;
        text.gameObject.SetActive(false);
    }

    private void ConfigureTopBarTextRect(TMP_Text text, Vector2 anchorMin, Vector2 anchorMax, float minSize, float maxSize, TextAlignmentOptions alignment)
    {
        if (text == null)
            return;

        if (topBarImage != null && text.transform.parent != topBarImage.transform)
            text.transform.SetParent(topBarImage.transform, false);

        ConfigureTextRect(text, anchorMin, anchorMax, minSize, maxSize, alignment);
        text.color = metallicTextColor;
        text.fontStyle = FontStyles.Bold;
        text.transform.SetAsLastSibling();
    }

    private void ConfigureTextRect(TMP_Text text, Vector2 anchorMin, Vector2 anchorMax, float minSize, float maxSize, TextAlignmentOptions alignment)
    {
        if (text == null)
            return;

        RectTransform rectTransform = text.rectTransform;
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        text.alignment = alignment;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.enableAutoSizing = true;
        text.fontSizeMin = minSize;
        text.fontSizeMax = maxSize;
        text.raycastTarget = false;
    }

    private GameObject FindSceneObject(string objectName)
    {
        if (string.IsNullOrEmpty(objectName))
            return null;

        Transform[] transforms = Resources.FindObjectsOfTypeAll<Transform>();

        for (int i = 0; i < transforms.Length; i++)
        {
            Transform candidate = transforms[i];
            if (candidate == null || candidate.name != objectName)
                continue;

            GameObject candidateObject = candidate.gameObject;
            if (candidateObject.hideFlags != HideFlags.None)
                continue;

            if (gameObject.scene.IsValid() && candidateObject.scene != gameObject.scene)
                continue;

            return candidateObject;
        }

        return null;
    }

    private string GetCardInfoLabel(CardData card)
    {
        if (card == null)
            return "";

        if (!string.IsNullOrWhiteSpace(card.bottomInfo))
            return card.bottomInfo;

        if (!string.IsNullOrWhiteSpace(card.cardTitle))
            return card.cardTitle;

        if (card.isReturnCard)
            return "Точка возврата";

        return GetHudContextLabel(card);
    }

    private string GetHudContextLabel(CardData currentCard)
    {
        if (currentCard == activeFinalCard)
            return "ФИНАЛ";

        if (currentCard != null && currentCard.isSpecialEvent)
            return "ОСОБОЕ СОБЫТИЕ";

        switch (currentPhase)
        {
            case GamePhase.Base:
                return "БАЗА";

            case GamePhase.RaidDay:
                return "ДНЕВНОЙ РЕЙД";

            case GamePhase.RaidNight:
                return "НОЧНОЙ РЕЙД";

            default:
                return "";
        }
    }

    private string GetStatusLabel()
    {
        string dataLabel = $"Данные: {surfaceData}/{requiredSurfaceData}";

        if (currentPhase == GamePhase.Base)
            return dataLabel;

        if (currentPhase == GamePhase.RaidDay)
            return $"Время: {raidTime}/{nightStartsAt} • Добыча: {currentLoot} • {dataLabel}";

        if (currentPhase == GamePhase.RaidNight)
            return $"Время: {raidTime} • Добыча: {currentLoot} • {dataLabel}";

        return dataLabel;
    }

    private void UpdateUI()
    {
        if (healthText != null) healthText.text = $"ЗДР\n{health}/{maxHealth}";
        if (staminaText != null) staminaText.text = $"ЭНР\n{stamina}/{maxStamina}";
        if (resourcesText != null) resourcesText.text = $"РЕС\n{resources}";
        if (moraleText != null) moraleText.text = $"МОР\n{morale}";

        RefreshHudState();
    }

    private void ClampStats()
    {
        health = Mathf.Clamp(health, 0, maxHealth);
        stamina = Mathf.Clamp(stamina, 0, maxStamina);
        resources = Mathf.Max(0, resources);
        morale = Mathf.Clamp(morale, -10, 10);
    }

    private string GetLoseReason()
    {
        if (health <= 0)
            return "Ты погиб во время вылазки.";

        if (stamina <= 0)
            return "Ты рухнул от истощения.";

        return null;
    }

    private void TriggerGameOver(string reason)
    {
        isGameOver = true;
        SetGameplayMenuButtonVisible(false);
        DeleteSave();

        SetGameOverPanelVisible(true);

        if (gameOverReasonText != null)
            gameOverReasonText.text = reason;
    }

    private void SetGameOverPanelVisible(bool isVisible)
    {
        if (gameOverPanel == null)
            return;

        gameOverPanel.SetActive(isVisible);

        if (isVisible)
            gameOverPanel.transform.SetAsLastSibling();
    }

    public void RestartGame()
    {
        DeleteSave();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
