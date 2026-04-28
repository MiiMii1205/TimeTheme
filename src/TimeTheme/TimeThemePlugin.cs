using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using BepInEx;
using BepInEx.Logging;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using HarmonyLib;
using PEAKLib.Core;
using pworld.Scripts.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zorro.Core;
using Zorro.Core.CLI;
using Zorro.Settings;

namespace TimeTheme;

[BepInAutoPlugin]
[BepInDependency(CorePlugin.Id)]
public partial class TimeThemePlugin : BaseUnityPlugin
{
    internal static ManualLogSource Log { get; private set; } = null!;
    public static TimeThemePlugin Instance { get; set; } = null!;

    public static Action<bool> OnThemeChanged;

    private bool m_isDay = true;
    private static Graphic[] m_graphs = [];
    private static Graphic[] m_settingsGraphs = [];
    private static Animator[] m_anims = [];

    private static Tuple<Color, Color> m_rebindDefaultTextColors = new(new Color(0.8745098f, 0.854902f, 0.7607843f, 1f),
        new Color(0.654902f, 0.3764706f, 0.7529412f, 1f));

    private static Tuple<Color, Color> m_rebindOverridenTextColors =
        new(new Color(0.8679245f, 0.7459151f, 0.3316127f, 1f), new Color(0.8901961f, 0.6313726f, 1f, 1f));

    private static Tuple<Color, Color>[] m_dayNightColors =
    [
        new(new Color(0.8742138f, 0.8567384f, 0.7615007f, 1f),
            new Color(0.6509804f, 0.3764706f, 0.7529412f, 1f)),
        new(new Color(0.745283f, 0.6866916f, 0.6573958f, 1f),
            new Color(0.5254902f, 0.1921569f, 0.6352941f, 1f)),
        new(new Color(0.8745099f, 0.854902f, 0.7607844f, 1f),
            new Color(0.6509804f, 0.37254903f, 0.7529412f, 1f)),
        new(new Color(0.408805f, 0.3755061f, 0.3329575f, 1f),
            new Color(0.3764706f, 0.3176471f, 0.6431373f, 1f)),
        new(new Color(0.3396226f, 0.2996456f, 0.241901f, 1f),
            new Color(0.3058824f, 0.254902f, 0.5176471f, 1f)),
        new(new Color(0.5849056f, 0.3446479f, 0.08552865f, 0.4823529f),
            new Color(0.3333333f, 0.3254902f, 0.8117647f, 0.4823529f)),
        new(new Color(0.990566f, 0.9718761f, 0.9485137f, 1f),
            new Color(0.772549f, 0.4745098f, 0.9294118f, 1f)),
        new(new Color(0.7735849f, 0.7577726f, 0.7261481f, 1f),
            new Color(0.772549f, 0.4745098f, 0.9294118f, 1f)),
        new(new Color(0.6289307f, 0.6142679f, 0.5438866f, 1f),
            new Color(0.4196078f, 0.1411765f, 0.5254902f, 1f)),
        new(new Color(0.9839351f, 1f, 0f, 1f),
            new Color(1f, 0.5568628f, 1f, 1f)),
        new(new Color(0.9119496f, 0.7547169f, 0f, 1f),
            new Color(0.9921569f, 0.4980392f, 1f, 1f)),
        new(new Color(1f, 0.9839351f, 0f, 1f),
            new Color(1f, 0.5607843f, 1f, 1f)),
        new(new Color(0.96855f, 0.83254f, 0.1797f, 1f),
            new Color(1f, 0.53333f, 1f, 1f)),
        new(new Color(0.2169367f, 0.2257782f, 0.2358491f, 1f),
            new Color(0.00392156862745098f, 0.00392156862745098f, 0.00392156862745098f, 1f)),
        new(new Color(0.8941177f, 0.8627452f, 0.8235295f, 1f),
            new Color(0.6745098f, 0.372549f, 0.8078431f, 1f)),
        new(new Color(0.2509804f, 0.2078432f, 0.2941177f, 1f),
            new Color(0.09803922f, 0.1058824f, 0.4352941f, 1f)),
        new(new Color(0.2503782f, 0.20830372f, 0.2924528f, 1f),
            new Color(0.1019608f, 0.1058824f, 0.427451f, 1f)),
        new(new Color(1f, 0.4840535f, 0.4559748f, 1f),
            new Color(0.972549f, 0.6627451f, 0.572549f, 1f)),
        new(new Color(0.8679245f, 0.7459151f, 0.3316127f, 1f), new Color(0.8901961f, 0.6313726f, 1f, 1f)),
        new(new Color(0.8745098f, 0.854902f, 0.7607843f, 1f),
            new Color(0.654902f, 0.3764706f, 0.7529412f, 1f)),
        new(new Color(0.1792453f, 0.1253449f, 0.09046815f, 0.7294118f),
        new Color(0f, 0f, 0f, 0.7294118f)),
    ];

    private static Tuple<Color, Color>[] m_dayNightOptionsColors =
    [
        new(new Color(1f, 1f, 1f, 1f),
            new Color(0.7843137f, 0.4980392f, 0.9764706f, 1f)),
        new(new Color(0.1792453f, 0.1253449f, 0.09046815f, 0.7294118f),
            new Color(0f, 0f, 0f, 0.7294118f)),
        new(new Color(0.9607843f, 0.9607843f, 0.9607843f, 1f),
            new Color(0.7450981f, 0.4627451f, 0.9372549f, 1f)),
        new(new Color(0.935506f, 0.7352661f, 0.2251314f, 1f),
            new Color(0.9921569f, 0.5921569f, 1f, 1f)),
        new(new Color(0.9607844f, 0.9607844f, 0.9607844f, 1f),
            new Color(0.7450981f, 0.4627451f, 0.9372549f, 1f)),
        new(new Color(0.7843137f, 0.7843137f, 0.7843137f, 0.5019608f),
            new Color(0.572549f, 0.2901961f, 0.7529412f, 0.5019608f)),
        new(new Color(0.8742138f, 0.8567384f, 0.7615007f, 1f),
            new Color(0.6509804f, 0.3764706f, 0.7529412f, 1f)),
        new(new Color(0.5157232f, 0.46707f, 0.410308f, 1f),
            new Color(0.3019608f, 0f, 0.3843137f, 1f)),
        new(new Color(0.7843137f, 0.7843137f, 0.7843137f, 1f),
            new Color(0.572549f, 0.2901961f, 0.7529412f, 1f)),
        new(new Color(1f, 0.629899f, 0f, 0.7686275f),
            new Color(0.8470588f, 0.5882353f, 1f, 1f)),
        new(new Color(0.95686f, 1f, 0f, 0.7615f),
            new Color(0.8470588f, 0.5882353f, 1f, 1f)),
    ];

    private static Tuple<Color, Color>[] m_dayNightButtonColors =
    [
        new(new Color(0.6603774f, 0.2775126f, 0.05918475f, 1f),
            new Color(0.3019608f, 0f, 0f, 1f)),
        new(new Color(0.8773585f, 0.4371066f, 0f, 1f),
            new Color(0.5058824f, 0.0627451f, 0f, 1f)),
        new(new Color(0.1415898f, 0.2924528f, 0.117257f, 1f), new Color(0f, 0.16470588235294117f, 0f, 1f)),
        new(new Color(0.05117479f, 0.4339623f, 0.1149727f, 1f),
            new Color(0.027450980392156862f, 0.24705882352941178f, 0.027450980392156862f, 1f)),
        new(new Color(0.7735849f, 0.5566919f, 0f, 1f), new Color(0.42745098039215684f, 0.23137254901960785f, 0f, 1f)),
        new(new Color(0.945098f, 0.7576478f, 0f, 1f),
            new Color(0.592156862745098f, 0.4117647058823529f, 0f, 1f)),
        new(new Color(0.1850303f, 0.3939696f, 0.6226415f, 1f),
            new Color(0f, 0.08235294117647059f, 0.2901960784313726f, 1)),
        new(new Color(0f, 0.5611874f, 0.5943396f, 1f), new Color(0f, 0.24313725490196078f, 0.2784313725490196f, 1f)),
        new(new Color(0.3918986f, 0.1843137f, 0.6235294f, 1f),
            new Color(0.11764705882352941f, 0f, 0.2784313725490196f, 1f)),
        new(new Color(0.5568805f, 0.1254894f, 0.8490566f, 1f),
            new Color(0.25098039215686274f, 0f, 0.4823529411764706f, 1f)),
        new(new Color(0.5188679f, 0.129717f, 0.1718048f, 1f), new Color(0.16470588235294117f, 0f, 0f, 1f)),
        new(new Color(0.745283f, 0.2057524f, 0f, 1f), new Color(0.3686274509803922f, 0f, 0f, 1f)),
        new(new Color(0.745283f, 0.6866916f, 0.6573958f, 1f),
            new Color(0.5254902f, 0.1921569f, 0.6352941f, 1f)),


        new(new Color(0.8207547f, 0.2682806f, 0.08904416f), new Color(0.4392156862745098f, 0f, 0f, 1f)),
        new(new Color(1f, 0.4262996f, 0.25f, 1f), new Color(0.611764705882353f, 0f, 0f, 1f)),

        new(new Color(0.4006706f, 0.6039216f, 0.1411764f, 1f),
            new Color(0.09411764705882353f, 0.2784313725490196f, 0f, 1f)),

        new(new Color(0.640078f, 0.8207547f, 0.01935742f, 1f),
            new Color(0.3176470588235294f, 0.47058823529411764f, 0f, 1f)),

        new(new Color(0.185f, 0.394f, 0.6226f, 1f),
            new Color(0f, 0.08235294117647059f, 0.28627450980392155f, 1f)),

        new(new Color(0.1255718f, 0.267434f, 0.4226f, 1f),
            new Color(0f, 0f, 0.11372549019607843f, 1f)),

        new(new Color(0.5189f, 0.1297f, 0.1718f, 1f),
            new Color(0.16470588235294117f, 0f, 0f, 1f)),

        new(new Color(0.3189f, 0.07970964f, 0.105583f, 1f),
            new Color(0.00784313725490196f, 0f, 0f, 1f)),

        new(new Color(0.3919f, 0.1843f, 0.6235f, 1f),
            new Color(0.11372549019607843f, 0f, 0.2784313725490196f, 1f)),

        new(new Color(0.2661904f, 0.1251821f, 0.4235f, 1f),
            new Color(0.023529411764705882f, 0f, 0.10588235294117647f, 1f)),

        new(new Color(0.6588235f, 0.2784314f, 0.05882353f, 1f),
            new Color(0.30196078431372547f, 0f, 0f, 1f)),

        new(new Color(0.8784314f, 0.4352941f, 0f, 1f),
            new Color(0.5058823529411764f, 0.06274509803921569f, 0f, 1f)),

        new(new Color(0.2741188f, 0.5283019f, 0.3919889f, 1f),
            new Color(0f, 0.21176470588235294f, 0.09803921568627451f, 1f)),

        new(new Color(0.4979019f, 0.5294118f, 0.2431372f, 1f),
            new Color(0.2f, 0.21568627450980393f, 0f, 1f)),

        new(new Color(0.24287f, 0.2515723f, 0.24287f, 1f),
            new Color(0.00784313725490196f, 0.00784313725490196f, 0.00784313725490196f, 1f)),

        new(new Color(0f, 0.2607184f, 0.3773585f, 1f),
            new Color(0f, 0f, 0.08235294117647059f, 1f)),

        new(new Color(0.509434f, 0.3279174f, 0.04645776f, 1f),
            new Color(0.19215686274509805f, 0.027450980392156862f, 0f, 1f)),

        new(new Color(0.5786163f, 0.172857f, 0.1758627f, 1f),
            new Color(0.2235294117647059f, 0f, 0f, 1f)),

        new(new Color(0.3673988f, 0.08415397f, 0.7232704f, 1f),
            new Color(0.12549019607843137f, 0f, 0.3607843137254902f, 1f)),

        new(new Color(0.6354969f, 0.6525992f, 0.7295597f, 1f),
            new Color(0.3137254901960784f, 0.32941176470588235f, 0.396078431372549f, 1f)),

        new(new Color(0.8f, 0.571995f, 0f, 1f),
            new Color(0.4549019607843137f, 0.24313725490196078f, 0f, 1f)),

        new(new Color(0.1803922f, 0.2941177f, 0.5019608f, 1f),
            new Color(0f, 0f, 0.1843137254901961f, 1f))
    ];

    private static readonly int Highlighted = Animator.StringToHash("Highlighted");
    private static readonly int Selected = Animator.StringToHash("Selected");
    private static readonly int Disabled = Animator.StringToHash("Disabled");
    private static readonly int Normal = Animator.StringToHash("Normal");
    private static readonly Color BannerDayColor = new Color(0.7075472f, 0.7075472f, 0.7075472f, 1f);

    private static readonly Color BannerNightColor = new Color(0.397f,
        0.397f, 0.397f, 1f);

    public AnimatorOverrideController DarkSlicesAnimator { get; set; } = null!;
    public AnimatorOverrideController DarkPipsAnimator { get; set; } = null!;
    public AnimatorOverrideController NightResetButtonAnimController { get; set; } = null!;
    public AnimatorOverrideController NightControlAnimController { get; set; } = null!;

    private void Awake()
    {
        Log = Logger;
        Instance = this;
        Log.LogInfo($"Time Theme {Name} is loaded!");

        this.LoadBundleAndContentsWithName("timetheme.peakbundle", bundle =>
        {
            DarkSlicesAnimator =
                bundle.LoadAsset<AnimatorOverrideController>("UI_DarkEmoteWheelSegment.overrideController");
            DarkPipsAnimator =
                bundle.LoadAsset<AnimatorOverrideController>("DarkSliderPip.overrideController");
            NightControlAnimController =
                bundle.LoadAsset<AnimatorOverrideController>("UI_DarkControl.overrideController");
            NightResetButtonAnimController =
                bundle.LoadAsset<AnimatorOverrideController>("DarkResetButton.overrideController");
            Log.LogInfo($"Time Theme bundle is loaded!");
        });

        m_nightInputFieldColorBlock = new ColorBlock()
        {
            highlightedColor = m_dayNightOptionsColors[2].Item2,
            normalColor = m_dayNightOptionsColors[0].Item2,
            disabledColor = m_dayNightOptionsColors[5].Item2,
            pressedColor = m_dayNightOptionsColors[8].Item2,
            selectedColor = m_dayNightOptionsColors[4].Item2,
            colorMultiplier = 1f,
            fadeDuration = 0.1f
        };

        m_defaultInputFieldColorBlock = new ColorBlock()
        {
            highlightedColor = m_dayNightOptionsColors[2].Item1,
            normalColor = m_dayNightOptionsColors[0].Item1,
            disabledColor = m_dayNightOptionsColors[5].Item1,
            pressedColor = m_dayNightOptionsColors[8].Item1,
            selectedColor = m_dayNightOptionsColors[4].Item1,
            colorMultiplier = 1f,
            fadeDuration = 0.1f
        };

        m_nightDropdownColorBlock = new ColorBlock()
        {
            highlightedColor = m_dayNightOptionsColors[2].Item2,
            normalColor = m_dayNightOptionsColors[0].Item2,
            disabledColor = m_dayNightOptionsColors[5].Item2,
            pressedColor = m_dayNightOptionsColors[8].Item2,
            selectedColor = m_dayNightOptionsColors[3].Item2,
            colorMultiplier = 1f,
            fadeDuration = 0.1f
        };

        m_defaultDropdownColorBlock = new ColorBlock()
        {
            highlightedColor = m_dayNightOptionsColors[2].Item1,
            normalColor = m_dayNightOptionsColors[0].Item1,
            disabledColor = m_dayNightOptionsColors[5].Item1,
            pressedColor = m_dayNightOptionsColors[8].Item1,
            selectedColor = m_dayNightOptionsColors[3].Item1,
            colorMultiplier = 1f,
            fadeDuration = 0.1f
        };

        var harmony = new Harmony(Id);

        harmony.PatchAll(typeof(TimeThemePatcher));
    }

    private void Update()
    {
        if (RunManager.Instance && RunManager.Instance.runStarted)
        {
            CheckSwitchTheme(DayNightManager.instance.isDay > 0.5f ||
                             MapHandler.Instance.GetCurrentSegment() >= Segment.Caldera);
        }
    }

    [ConsoleCommand]
    public static void SwitchTheme(bool toDay)
    {
        Instance.CheckSwitchTheme(toDay);
    }

#if DEBUG

    [ConsoleCommand]
    public static void ShowTestHero()
    {
        GUIManager.instance.SetHeroTitle("IS THIS THING WORKING NOW? OMFG",
            MountainProgressHandler.Instance.progressPoints[0].clip);
    }
#endif

    private void CheckSwitchTheme(bool isDay)
    {
        if (m_isDay != isDay)
        {
            if (m_graphs.Length <= 0)
            {
                RefreshGraphicLists();
            }

            StartCoroutine(SwitchDayNight(isDay, m_isDay));
            m_isDay = isDay;
            OnThemeChanged?.Invoke(m_isDay);
        }
    }

    private static TweenerCore<Color, Color, ColorOptions> DoReticuleColor(
        Color endValue,
        float duration)
    {
        var t = DOTween.To((DOGetter<Color>) (() =>
                GUIManager.instance.reticleColorDefault),
            (DOSetter<Color>) (x => GUIManager.instance.reticleColorDefault = x), endValue, duration);

        t.SetTarget(GUIManager.instance.reticleColorDefault);

        return t;
    }

    private static TweenerCore<Color, Color, ColorOptions> DoPauseMenuRebind(
        PauseMenuRebindButton pmrb,
        Color endValue,
        float duration)
    {
        var t = DOTween.To((DOGetter<Color>) (() =>
                pmrb.defaultTextColor),
            (DOSetter<Color>) (x => pmrb.defaultTextColor = x), endValue, duration);

        t.SetTarget(pmrb.defaultTextColor);

        return t;
    }

    private static TweenerCore<Color, Color, ColorOptions> DoPauseMenuRebindOverriden(
        PauseMenuRebindButton pmrb,
        Color endValue,
        float duration)
    {
        var t = DOTween.To((DOGetter<Color>) (() =>
                pmrb.overriddenTextColor),
            (DOSetter<Color>) (x => pmrb.overriddenTextColor = x), endValue, duration);

        t.SetTarget(pmrb.overriddenTextColor);

        return t;
    }

    private static TweenerCore<Color, Color, ColorOptions> DoReticuleHighlightColor(
        Color endValue,
        float duration)
    {
        var t = DOTween.To((DOGetter<Color>) (() =>
                GUIManager.instance.reticleColorHighlight),
            (DOSetter<Color>) (x => GUIManager.instance.reticleColorHighlight = x), endValue, duration);

        t.SetTarget(GUIManager.instance.reticleColorHighlight);

        return t;
    }

    private static TweenerCore<Color, Color, ColorOptions> DoSpectateColor(
        Color endValue,
        float duration)
    {
        var t = DOTween.To((DOGetter<Color>) (() =>
                GUIManager.instance.spectatingNameColor),
            (DOSetter<Color>) (x => GUIManager.instance.spectatingNameColor = x), endValue, duration);

        t.SetTarget(GUIManager.instance.spectatingNameColor);

        return t;
    }

    private static TweenerCore<Color, Color, ColorOptions> DoSpectateYouColor(
        Color endValue,
        float duration)
    {
        var t = DOTween.To((DOGetter<Color>) (() =>
                GUIManager.instance.spectatingYourselfColor),
            (DOSetter<Color>) (x => GUIManager.instance.spectatingYourselfColor = x), endValue, duration);

        t.SetTarget(GUIManager.instance.spectatingYourselfColor);

        return t;
    }

    private Coroutine? m_gUpdates;

    private void DoGraphicUpdates(bool isDay, bool wasDay)
    {
        if (m_gUpdates != null)
        {
            StopCoroutine(m_gUpdates);
        }

        m_gUpdates = StartCoroutine(UpdateGraphics(isDay, wasDay));
    }

    private Coroutine? m_aUpdates;
    private Coroutine? m_apUpdates;
    private ColorBlock m_defaultInputFieldColorBlock;
    private ColorBlock m_nightInputFieldColorBlock;
    private ColorBlock m_defaultDropdownColorBlock;
    private ColorBlock m_nightDropdownColorBlock;
    private static Slider[] m_prefabPips;
    private static SharedSettingsMenu m_settingTab;
    private static GameObject[] m_settingsCells;

    private void DoAnimsUpdates(bool isDay, bool wasDay)
    {
        if (m_aUpdates != null)
        {
            StopCoroutine(m_aUpdates);
        }

        m_aUpdates = StartCoroutine(UpdateAnims(isDay, wasDay));
    }

    private void DoPipAnimsUpdates(bool isDay, bool wasDay)
    {
        if (m_apUpdates != null)
        {
            StopCoroutine(m_apUpdates);
        }

        m_apUpdates = StartCoroutine(UpdatePipsAnims(isDay, wasDay));
    }

    private IEnumerator SwitchDayNight(bool isDay, bool wasDay)
    {
        DoReticuleColor(isDay ? m_dayNightColors[0].Item1 : m_dayNightColors[0].Item2, 0.25f);
        DoReticuleHighlightColor(isDay ? m_dayNightColors[10].Item1 : m_dayNightColors[10].Item2, 0.25f);
        DoSpectateColor(isDay ? m_dayNightColors[0].Item1 : m_dayNightColors[0].Item2, 0.25f);
        DoSpectateYouColor(isDay ? m_dayNightColors[11].Item1 : m_dayNightColors[11].Item2, 0.25f);

        yield return null;

        DoGraphicUpdates(isDay, wasDay);
        DoAnimsUpdates(isDay, wasDay);
        DoPipAnimsUpdates(isDay, wasDay);
    }

    private IEnumerator UpdateAnims(bool isDay, bool wasDay)
    {
        var oldAnimName = wasDay ? "UI_EmoteWheelSegment" : "UI_DarkEmoteWheelSegment";
        

        for (var i = 0; i < m_anims.Length; i++)
        {
            if (m_anims[i] is { } a)
            {
                
                var animatorName = a.runtimeAnimatorController.name.Replace("(Instance)", "").Replace("(Clone)", "");
                
                switch (animatorName)
                {
                    case "UI_EmoteWheelSegment":
                    case "UI_DarkEmoteWheelSegment":
                    {
                        oldAnimName = wasDay ? "UI_EmoteWheelSegment" : "UI_DarkEmoteWheelSegment";
                        if (animatorName == oldAnimName)
                        {
                            if (animatorName == "UI_EmoteWheelSegment" && DayAnimController == null)
                            {
                                DayAnimController = Instantiate(a.runtimeAnimatorController);
                            }

                            var newAnim = Instantiate(isDay ? DayAnimController : DarkSlicesAnimator);

                            a.runtimeAnimatorController = newAnim;

                            if (a.IsPlaying("Highlighted") || a.IsPlaying("A_DarkHighlighted"))
                            {
                                a.SetTrigger(Highlighted);
                            }
                            else if (a.IsPlaying("Selected"))
                            {
                                a.SetTrigger(Selected);
                            }
                            else if (a.IsPlaying("Disabled") || a.IsPlaying("A_DarkDisabled"))
                            {
                                a.SetTrigger(Disabled);
                            }
                            else
                            {
                                a.SetTrigger(Normal);
                            }

                            yield return null;
                        }
                    }
                        break;

                    case "ResetButton":
                    case "DarkResetButton":
                    {
                         oldAnimName = wasDay ? "ResetButton" : "DarkResetButton";
                        if (animatorName == oldAnimName)
                        {
                            if (animatorName == "ResetButton" && DayResetButtonAnimController == null)
                            {
                                DayResetButtonAnimController = Instantiate(a.runtimeAnimatorController);
                            }

                            var newAnim = Instantiate(isDay ? DayResetButtonAnimController : NightResetButtonAnimController);

                            a.runtimeAnimatorController = newAnim;

                            if (a.IsPlaying("Highlighted") || a.IsPlaying("A_DarkHighlighted"))
                            {
                                a.SetTrigger(Highlighted);
                            }
                            else if (a.IsPlaying("Selected"))
                            {
                                a.SetTrigger(Selected);
                            }
                            else if (a.IsPlaying("Disabled") || a.IsPlaying("A_DarkDisabled"))
                            {
                                a.SetTrigger(Disabled);
                            }
                            else
                            {
                                a.SetTrigger(Normal);
                            }

                            yield return null;
                        }
                    }
                        break;
                    
                    case "UI_DarkControl":
                    case "UI_Control":
                    {
                        oldAnimName = wasDay ? "UI_Control" : "UI_DarkControl";
                        if (animatorName == oldAnimName)
                        {
                            if (animatorName == "UI_Control" && DayControlAnimController == null)
                            {
                                DayControlAnimController = Instantiate(a.runtimeAnimatorController);
                            }

                            var newAnim = Instantiate(isDay ? DayControlAnimController : NightControlAnimController);

                            a.runtimeAnimatorController = newAnim;

                            if (a.IsPlaying("Highlighted") || a.IsPlaying("A_DarkHighlighted"))
                            {
                                a.SetTrigger(Highlighted);
                            }
                            else if (a.IsPlaying("Selected"))
                            {
                                a.SetTrigger(Selected);
                            }
                            else if (a.IsPlaying("Disabled") || a.IsPlaying("A_DarkDisabled"))
                            {
                                a.SetTrigger(Disabled);
                            }
                            else
                            {
                                a.SetTrigger(Normal);
                            }

                            yield return null;
                        }
                    }
                        break;
                    
                    
                }
                

                
            }
        }
    }

    private IEnumerator UpdatePipsAnims(bool isDay, bool wasDay)
    {
        var oldAnimName = wasDay ? "SliderPip" : "DarkSliderPip";

        var sliderPips = m_settingTab.m_settingsContentParent.GetComponentsInChildren<Slider>(true)
            .AddRangeToArray(m_prefabPips);

        for (var i = 0; i < sliderPips.Length; i++)
        {
            if (sliderPips[i].TryGetComponent(out Animator a))
            {
                var animatorName = a.runtimeAnimatorController.name.Replace("(Instance)", "").Replace("(Clone)", "");

                if (animatorName == oldAnimName)
                {
                    if (animatorName == "SliderPip" && DaySliderAnimController == null)
                    {
                        DaySliderAnimController = Instantiate(a.runtimeAnimatorController);
                    }

                    var newAnim = Instantiate(isDay ? DaySliderAnimController : DarkPipsAnimator);

                    a.runtimeAnimatorController = newAnim;

                    if (a.IsPlaying("Highlighted") || a.IsPlaying("A_DarkPipHighlighted"))
                    {
                        a.SetTrigger(Highlighted);
                    }
                    else if (a.IsPlaying("Selected") || a.IsPlaying("A_DarkPipSelected"))
                    {
                        a.SetTrigger(Selected);
                    }
                    else if (a.IsPlaying("Disabled") || a.IsPlaying("A_DarkPipDisabled"))
                    {
                        a.SetTrigger(Disabled);
                    }
                    else
                    {
                        a.SetTrigger(Normal);
                    }

                    yield return null;
                }
            }
        }
    }


    private IEnumerator UpdateGraphics(bool isDay, bool wasDay)
    {
        HashSet<int> changedUIControl = new();

        for (var i = 0; i < m_graphs.Length; i++)
        {
            var usedArray = m_dayNightColors;
            Tweener? t = null;

            if (m_graphs[i] is Image image && ((image.sprite &&
                                                image.sprite.name is "UI_Banner" or "UI_Blur_DoubleArrow"
                                                    or "UI_Blur_Arrow" or "DottedLine") || image.name == "BadgeSash"))
            {
                usedArray = m_dayNightButtonColors;
            }

            if (m_graphs[i] is TextMeshProUGUI && m_graphs[i].name is "HeroText" or "HeroTimeOfDay" or "HeroDay")
            {
                continue;
            }

            if (m_graphs[i] is TextMeshProUGUI && m_graphs[i].transform.parent.name.StartsWith("UI_Control") &&
                m_graphs[i].transform.parent.TryGetComponent(out PauseMenuRebindButton pauseMenuRebindButton) &&
                !changedUIControl.Contains(pauseMenuRebindButton.transform.GetInstanceID()))
            {
                if (m_graphs[i].isActiveAndEnabled)
                {
                    DoPauseMenuRebind(pauseMenuRebindButton,
                        isDay ? m_rebindDefaultTextColors.Item1 : m_rebindDefaultTextColors.Item2, 0.25f);
                    t = DoPauseMenuRebindOverriden(pauseMenuRebindButton,
                        isDay ? m_rebindOverridenTextColors.Item1 : m_rebindOverridenTextColors.Item2, 0.25f);
                }
                else
                {
                    pauseMenuRebindButton.defaultTextColor =
                        isDay ? m_rebindDefaultTextColors.Item1 : m_rebindDefaultTextColors.Item2;

                    pauseMenuRebindButton.overriddenTextColor =
                        isDay ? m_rebindOverridenTextColors.Item1 : m_rebindOverridenTextColors.Item2;
                }

                changedUIControl.Add(pauseMenuRebindButton.GetInstanceID());
            }

            if (m_graphs[i] is Image bim && bim.sprite &&
                bim.sprite.name is "EndgameBanner" or "EndgameBannerLose" &&
                m_graphs[i].transform.parent.name.StartsWith("Banner"))
            {
                var newColor = isDay ? BannerDayColor : BannerNightColor;
                var c = new Color(newColor.r, newColor.g, newColor.b, m_graphs[i].color.a);
                if (m_graphs[i].isActiveAndEnabled)
                {
                    t = m_graphs[i].DOColor(c, 0.25f);
                }
                else
                {
                    m_graphs[i].color = c;
                }
            }
            else
            {
                for (var i1 = 0; i1 < usedArray.Length; ++i1)
                {
                    
                    if (i1 == 20 && m_graphs[i].name is "Background" or "BG")
                    {
                        continue;
                    }
                    
                    var oldColor = wasDay ? usedArray[i1].Item1 : usedArray[i1].Item2;
                    var newColor = isDay ? usedArray[i1].Item1 : usedArray[i1].Item2;
                    
                    if (CompareColors(m_graphs[i].color, oldColor))
                    {
                        var c = new Color(newColor.r, newColor.g, newColor.b, m_graphs[i].color.a);
                        if (m_graphs[i].isActiveAndEnabled)
                        {
                            t = m_graphs[i].DOColor(c, 0.25f);
                        }
                        else
                        {
                            m_graphs[i].color = c;
                        }

                        break;
                    }
                }
            }

            if (t != null)
            {
                yield return null;
            }
        }

        var graphs = m_settingTab.m_settingsContentParent.transform.GetComponentsInChildren<Graphic>(true)
            .AddRangeToArray(m_settingsGraphs);

        for (var i = 0; i < graphs.Length; i++)
        {
            if (graphs[i].name.StartsWith("Input") && graphs[i].TryGetComponent(out TMP_InputField field))
            {
                if (graphs[i].isActiveAndEnabled)
                {
                    DoColorBlocks(field, m_dayNightOptionsColors, isDay, wasDay);
                }
                else
                {
                    field.colors = isDay ? m_defaultInputFieldColorBlock : m_nightInputFieldColorBlock;
                }
            }
            else if (graphs[i].name == "Dropdown" && graphs[i].TryGetComponent(out TMP_Dropdown dr))
            {
                if (graphs[i].isActiveAndEnabled)
                {
                    DoColorBlocks(dr, m_dayNightOptionsColors, isDay, wasDay);
                }
                else
                {
                    dr.colors = isDay ? m_defaultDropdownColorBlock : m_nightDropdownColorBlock;
                }
            }
            else
            {
                if (graphs[i].name == "Handle")
                {
                    // new anims don't set defaults. We have to set it ourselves
                    var oldColor = wasDay ? m_dayNightOptionsColors[6].Item1 : m_dayNightOptionsColors[6].Item2;
                    var newColor = isDay ? m_dayNightOptionsColors[6].Item1 : m_dayNightOptionsColors[6].Item2;

                    var c = new Color(newColor.r, newColor.g, newColor.b, graphs[i].color.a);

                    if (graphs[i].isActiveAndEnabled)
                    {
                        graphs[i].DOColor(c, 0.25f);
                    }
                    else
                    {
                        graphs[i].color = c;
                    }

                    continue;
                }

                for (var i1 = 0; i1 < m_dayNightOptionsColors.Length; ++i1)
                {
                    var oldColor = wasDay ? m_dayNightOptionsColors[i1].Item1 : m_dayNightOptionsColors[i1].Item2;
                    var newColor = isDay ? m_dayNightOptionsColors[i1].Item1 : m_dayNightOptionsColors[i1].Item2;

                    var col = (graphs[i].color).PToVec4();

                    if (new Vector3(col.x, col.y, col.z) == new Vector3(oldColor.r, oldColor.g, oldColor.b))
                    {
                        var c = new Color(newColor.r, newColor.g, newColor.b, graphs[i].color.a);
                        if (graphs[i].isActiveAndEnabled)
                        {
                            graphs[i].DOColor(c, 0.25f);
                        }
                        else
                        {
                            graphs[i].color = c;
                        }

                        break;
                    }
                }
            }

            yield return null;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool CompareColors(Color a, Color b)
    {
        return new Vector3(a.r, a.g, a.b) == new Vector3(b.r, b.g, b.b);
    }

    private static void DoColorBlocks(Selectable selectable, Tuple<Color, Color>[] colors, bool isDay, bool wasDay)
    {
        for (var i1 = 0; i1 < colors.Length; ++i1)
        {
            var oldColor = wasDay ? colors[i1].Item1 : colors[i1].Item2;
            var newColor = isDay ? colors[i1].Item1 : colors[i1].Item2;

            if (CompareColors(selectable.colors.normalColor, oldColor))
            {
                var t = DOTween.To((DOGetter<Color>) (() =>
                        selectable.colors.normalColor),
                    (DOSetter<Color>) (x => selectable.colors = selectable.colors with {normalColor = x}), newColor,
                    0.25f);

                t.SetTarget(selectable.colors.normalColor);
            }

            if (CompareColors(selectable.colors.disabledColor, oldColor))
            {
                var t = DOTween.To((DOGetter<Color>) (() =>
                        selectable.colors.disabledColor),
                    (DOSetter<Color>) (x => selectable.colors = selectable.colors with {disabledColor = x}), newColor,
                    0.25f);

                t.SetTarget(selectable.colors.disabledColor);
            }

            if (CompareColors(selectable.colors.highlightedColor, oldColor))
            {
                var t = DOTween.To((DOGetter<Color>) (() =>
                        selectable.colors.highlightedColor),
                    (DOSetter<Color>) (x => selectable.colors = selectable.colors with {highlightedColor = x}),
                    newColor,
                    0.25f);

                t.SetTarget(selectable.colors.highlightedColor);
            }

            if (CompareColors(selectable.colors.selectedColor, oldColor))
            {
                var t = DOTween.To((DOGetter<Color>) (() =>
                        selectable.colors.selectedColor),
                    (DOSetter<Color>) (x => selectable.colors = selectable.colors with {selectedColor = x}), newColor,
                    0.25f);

                t.SetTarget(selectable.colors.selectedColor);
            }

            if (CompareColors(selectable.colors.pressedColor, oldColor))
            {
                var t = DOTween.To((DOGetter<Color>) (() =>
                        selectable.colors.pressedColor),
                    (DOSetter<Color>) (x => selectable.colors = selectable.colors with {pressedColor = x}), newColor,
                    0.25f);

                t.SetTarget(selectable.colors.pressedColor);
            }
        }
    }

    public RuntimeAnimatorController DayAnimController
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set;
    }
    public RuntimeAnimatorController DayControlAnimController
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set;
    }
    public RuntimeAnimatorController DayResetButtonAnimController
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set;
    }

    public RuntimeAnimatorController DaySliderAnimController
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set;
    }

    public static Color DayTabsBackground
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get=> m_dayNightOptionsColors[1].Item1;
    }
    public static Color NightTabsBackground
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get=> m_dayNightOptionsColors[1].Item2;
    }

    public static Color DarkTabText
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => m_dayNightOptionsColors[0].Item2;
    }

    public static bool IsDarkTheme
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => !Instance.m_isDay;
    }

    public static void RefreshGraphicLists()
    {
        m_graphs = GUIManager.instance.transform.GetComponentsInChildren<Graphic>(true);
        m_anims = GUIManager.instance.transform.GetComponentsInChildren<Animator>(true);
        m_settingTab = GUIManager.instance.transform.GetComponentInChildren<SharedSettingsMenu>(true);
        m_settingsGraphs = m_settingTab.m_settingsCellPrefab.GetComponentsInChildren<Graphic>(true);
        m_settingsGraphs =
            m_settingsGraphs.AddRangeToArray(m_settingTab.transform.GetComponentsInChildren<Graphic>(true));
        m_prefabPips = [];

        var c = SingletonAsset<InputCellMapper>.Instance;

        // Unrolled

        if (c?.EnumSettingCell)
        {
            m_settingsGraphs =
                m_settingsGraphs
                    .AddRangeToArray(c.EnumSettingCell.transform
                        .GetComponentsInChildren<Graphic>(true));

            m_prefabPips = m_prefabPips.AddRangeToArray(c.EnumSettingCell.transform
                .GetComponentsInChildren<Slider>(true));
        }

        if (c?.ButtonSettingCell)
        {
            m_settingsGraphs =
                m_settingsGraphs
                    .AddRangeToArray(c.ButtonSettingCell.transform
                        .GetComponentsInChildren<Graphic>(true));

            m_prefabPips = m_prefabPips.AddRangeToArray(c.ButtonSettingCell.transform
                .GetComponentsInChildren<Slider>(true));
        }

        if (c?.FloatSettingCell)
        {
            m_settingsGraphs =
                m_settingsGraphs
                    .AddRangeToArray(c.FloatSettingCell.transform
                        .GetComponentsInChildren<Graphic>(true));

            m_prefabPips = m_prefabPips.AddRangeToArray(c.FloatSettingCell.transform
                .GetComponentsInChildren<Slider>(true));
        }


        if (c?.StringSettingCell)
        {
            m_settingsGraphs =
                m_settingsGraphs
                    .AddRangeToArray(c.StringSettingCell.transform
                        .GetComponentsInChildren<Graphic>(true));

            m_prefabPips = m_prefabPips.AddRangeToArray(c.StringSettingCell.transform
                .GetComponentsInChildren<Slider>(true));
        }


        if (c?.IntSettingCell)
        {
            m_settingsGraphs =
                m_settingsGraphs
                    .AddRangeToArray(c.IntSettingCell.transform
                        .GetComponentsInChildren<Graphic>(true));

            m_prefabPips = m_prefabPips.AddRangeToArray(c.IntSettingCell.transform
                .GetComponentsInChildren<Slider>(true));
        }

        if (c?.KeyCodeSettingCell)
        {
            m_settingsGraphs =
                m_settingsGraphs
                    .AddRangeToArray(c.KeyCodeSettingCell.transform
                        .GetComponentsInChildren<Graphic>(true));

            m_prefabPips = m_prefabPips.AddRangeToArray(c.KeyCodeSettingCell.transform
                .GetComponentsInChildren<Slider>(true));
        }
    }
}