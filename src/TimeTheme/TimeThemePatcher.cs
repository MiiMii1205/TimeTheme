using DG.Tweening;
using HarmonyLib;
using pworld.Scripts.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace TimeTheme;

public class TimeThemePatcher
{
    private static readonly Vector3 _dc = TimeThemePlugin.DayTabsBackground.PToVec4();
    private static readonly Vector3 _nc = TimeThemePlugin.NightTabsBackground.PToVec4();

    [HarmonyPatch(typeof(RunManager), nameof(RunManager.StartRun))]
    [HarmonyPostfix]
    public static void RunManagerAwakePostfix(RunManager __instance)
    {
        TimeThemePlugin.RefreshGraphicLists();
        TimeThemePlugin.SwitchTheme(DayNightManager.instance.isDay > 0.5f ||
                                    MapHandler.Instance.GetCurrentSegment() >= Segment.Caldera);
    }
    
    [HarmonyPatch(typeof(SettingsTABSButton), nameof(SettingsTABSButton.Update))]
    [HarmonyPrefix]
    public static void RunManagerAwakePostfix(SettingsTABSButton __instance, ref bool __runOriginal)
    {
        if (TimeThemePlugin.IsDarkTheme && !__instance.Selected)
        {
            __runOriginal = false;
            
            __instance.text.color = Color.Lerp(__instance.text.color,
                TimeThemePlugin.DarkTabText, Time.unscaledDeltaTime * 7f);

            __instance.background.color = Color.Lerp(__instance.background.color,
                TimeThemePlugin.NightTabsBackground, Time.unscaledDeltaTime * 7f);
            
            __instance.SelectedGraphic.gameObject.SetActive(__instance.Selected);

        }
    }
    
    
    
}