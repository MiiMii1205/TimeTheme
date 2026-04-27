using DG.Tweening;
using HarmonyLib;

namespace TimeTheme;

public class TimeThemePatcher
{
    [HarmonyPatch(typeof(RunManager), nameof(RunManager.StartRun))]
    [HarmonyPostfix]
    public static void RunManagerAwakePostfix(RunManager __instance)
    {
        TimeThemePlugin.RefreshGraphicLists();
        TimeThemePlugin.SwitchTheme(DayNightManager.instance.isDay > 0.5f ||
                                    MapHandler.Instance.GetCurrentSegment() >= Segment.Caldera);
    }      
    
}