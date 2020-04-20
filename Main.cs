using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using HarmonyLib;
using UnityEngine.UI;
using System.Reflection;

namespace Circle_Empires_Editor
{

	[BepInPlugin("Yan.CircleEmpiresEditor", "Circle Empires Editor", "0.4.2")]
	public class Circle_Empires_Editor : BaseUnityPlugin
	{
        private static GameObject manager = null;
        public static ManageUI manageUI = null;
        public static General general = null;

        public ConfigEntry<KeyboardShortcut> Hotkey_UpLevel { get; private set; }
        public ConfigEntry<KeyboardShortcut> Hotkey_SetSpeed { get; private set; }
        public ConfigEntry<KeyboardShortcut> Hotkey_MindControl { get; private set; }

        private int timeMindControl = 100;
        private MethodInfo ThingShowFreezeVisual;

        private Vector2 ClampWindowSize(Vector2 orig)
        {
            return new Vector2(Mathf.Clamp(orig.x, Mathf.Min(960, Screen.width), Screen.width), Mathf.Clamp(orig.y, Mathf.Min(720, Screen.height), Screen.height));
        }

        public bool Initialize()
        {
            try
            {
                if (manager == null)
                {
                    manager = GameObject.FindGameObjectWithTag("Manager");
                    manageUI = manager.GetComponent<ManageUI>();
                    general = manager.GetComponent<General>();
                    ThingShowFreezeVisual = AccessTools.Method(typeof(Thing), "ShowFreezeVisual");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return false;
            }
            return true;
        }

        private void UpLevel()
        {
            Logger.Log(LogLevel.Info, "UpLevel");

            if (manageUI.selectedThings.Count >= 1)
            {
                for (int i = 0; i < manageUI.selectedThings.Count;)
                {
                    if (manageUI.selectedThings[i].GainXp(100000000) == 0)
                        i++;
                }
            }
            else
                Logger.LogWarning("Please Select Thing");

        }

        private void SetSpeed()
        {
            Logger.Log(LogLevel.Info, "SetSpeed");

            if (manageUI.selectedThings.Count >= 1)
            {
                foreach(var i in manageUI.selectedThings)
                {
                    ThingShowFreezeVisual.Invoke(i, new object[] { });
                    i.freezeDuration = 100000000;
                    i.freezeFactor = 1000000;
                }
            }
            else
                Logger.LogWarning("Please Select Thing");

        }

        private void Update()
        {
            if (Hotkey_UpLevel.Value.IsDown())
                UpLevel();
            if (Hotkey_SetSpeed.Value.IsDown())
                SetSpeed();
            if (Hotkey_MindControl.Value.IsDown())
            {
                RuntimeConfig.ChangePlayerEnable = !RuntimeConfig.ChangePlayerEnable;
                timeMindControl = 0;
                Logger.Log(LogLevel.Info, "Mind Control " + ( RuntimeConfig.ChangePlayerEnable ? "Enable" : "Disable"));
            }
            if(timeMindControl < 100)
            {
                manageUI.ShowToolTip(10, Screen.height - 10, "Mind Control " + (RuntimeConfig.ChangePlayerEnable ? "Enable" : "Disable"));
                timeMindControl++;
            }
        }

        private void Awake()
        {
            DontDestroyOnLoad(this);

            Hotkey_UpLevel = Config.Bind("Cheat-Hotkey", "Unit Level Up Cheat", new KeyboardShortcut(KeyCode.F12, new KeyCode[] { KeyCode.LeftControl }));
            Hotkey_SetSpeed = Config.Bind("Cheat-Hotkey", "Unit Speed Cheat", new KeyboardShortcut(KeyCode.F9, new KeyCode[] { KeyCode.LeftControl }));
            Hotkey_MindControl = Config.Bind("Cheat-Hotkey", "Mind Control Cheat", new KeyboardShortcut(KeyCode.F8, new KeyCode[] { KeyCode.LeftControl }));

            RuntimeConfig.NoIntroVideo = Config.Bind("OtherFunction", "NoIntroVideo", true, "Prevent the intro video from playing on startup.").Value;

            HarmonyPatches.Initialize(this);
        }
    }

    public static class HarmonyPatches
    {
        public static readonly Harmony harmony = new Harmony("Yan.CircleEmpiresEditor");
        public static readonly Type thisType = typeof(HarmonyPatches);
        

        public static void ShowTooltipPostfix(Thing __instance)
        {
            if (RuntimeConfig.ChangePlayerEnable && 
                __instance.player != __instance.general.locallyControlledPlayer)
                __instance.ChangePlayerTo(__instance.general.locallyControlledPlayer);
        }

        public static void IntroVideoUpdatePrefix(IntroVideo __instance)
        {
            if (__instance.startDone == 0 && __instance.manageContent.appInitializationDone > 0 && RuntimeConfig.NoIntroVideo)
            {
                __instance.startDone = 1;
                RuntimeConfig.Circle_Empires_Editor.Initialize();
                Button component = __instance.transform.parent.Find("CloseIntroVideoButton").gameObject.GetComponent<Button>();
                component.onClick.Invoke();
            }
        }

        public static void Initialize(Circle_Empires_Editor instance)
        {
            harmony.Patch(AccessTools.Method(typeof(Thing), "ShowTooltip"),null, new HarmonyMethod(thisType, "ShowTooltipPostfix"));
            harmony.Patch(AccessTools.Method(typeof(IntroVideo), "Update"), new HarmonyMethod(thisType, "IntroVideoUpdatePrefix"));
            RuntimeConfig.Circle_Empires_Editor = instance;
        }
    }

    public static class RuntimeConfig
    {
        public static bool ChangePlayerEnable = false;
        public static bool NoIntroVideo = true;
        public static Circle_Empires_Editor Circle_Empires_Editor = null;
    }


    //public class SettingUI : MonoBehaviour
    //{

    //}

}
