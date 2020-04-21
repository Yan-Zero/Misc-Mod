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
	[BepInPlugin("Yan.CircleEmpiresEditor", "Circle Empires Editor", "0.4.5")]
	public class Circle_Empires_Editor : BaseUnityPlugin
	{
        public new ManualLogSource Logger;

        private static GameObject manager = null;
        public static ManageUI manageUI = null;
        public static General general = null;

        public static GameObject SettingUI = null;

        public ConfigEntry<KeyboardShortcut> Hotkey_UpLevel { get; private set; }
        public ConfigEntry<KeyboardShortcut> Hotkey_SetSpeed { get; private set; }
        public ConfigEntry<KeyboardShortcut> Hotkey_MindControl { get; private set; }
        public ConfigEntry<KeyboardShortcut> Hotkey_ToggleWindows { get; private set; }

        private int timeMindControl = 100;
        private MethodInfo ThingShowFreezeVisual;


        Circle_Empires_Editor()
        {
            Logger = base.Logger;
        }

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

                    RuntimeConfig.FontConfig.Signika_Bold = Circle_Empires_Editor.manageUI.settingsMenu.GetComponent<RectTransform>().Find("Title").gameObject.GetComponent<Text>().font;
                    RuntimeConfig.SpriteConfig.BackImage = Circle_Empires_Editor.manageUI.settingsMenu.GetComponent<Image>();
                    RuntimeConfig.SpriteConfig.Button_Brown = Circle_Empires_Editor.manageUI.settingsMenu.transform.Find("Back").gameObject.GetComponent<Image>();

                    SettingUI = new GameObject("Yan.SettingUI");

                    var SettingUIRectTransform = SettingUI.AddComponent<RectTransform>();
                    SettingUIRectTransform.sizeDelta = new Vector2(1000,750);
                    SettingUIRectTransform.SetParent(manageUI.settingsMenu.transform.parent);
                    SettingUIRectTransform.anchoredPosition = new Vector2(0, 0);
                    SettingUI.AddComponent<SettingUI>();
                    SettingUI.SetActive(true);
                    
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
            if(Hotkey_ToggleWindows.Value.IsDown())
            {
                SettingUI.SetActive(!SettingUI.activeSelf);
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

            Hotkey_UpLevel = Config.Bind("Cheat-Hotkey", "Unit Level Up Cheat", new KeyboardShortcut(KeyCode.F7, new KeyCode[] { KeyCode.LeftControl }));
            Hotkey_SetSpeed = Config.Bind("Cheat-Hotkey", "Unit Speed Cheat", new KeyboardShortcut(KeyCode.F9, new KeyCode[] { KeyCode.LeftControl }));
            Hotkey_MindControl = Config.Bind("Cheat-Hotkey", "Mind Control Cheat", new KeyboardShortcut(KeyCode.F8, new KeyCode[] { KeyCode.LeftControl }));

            RuntimeConfig.NoIntroVideo = Config.Bind("OtherFunction", "NoIntroVideo", true, "Prevent the intro video from playing on startup.").Value;
            Hotkey_ToggleWindows = Config.Bind("Hotkey", "ToggleWindows", new KeyboardShortcut(KeyCode.F12, new KeyCode[] { KeyCode.LeftControl }), "Open or close setting windows.");

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

        public static void ManageUIAwakePostfix()
        {

            RuntimeConfig.Circle_Empires_Editor.Initialize();
        }

        public static void IntroVideoUpdatePrefix(IntroVideo __instance)
        {
            if (__instance.startDone == 0 && __instance.manageContent.appInitializationDone > 0 && RuntimeConfig.NoIntroVideo)
            {
                __instance.startDone = 1;
                Button component = __instance.transform.parent.Find("CloseIntroVideoButton").gameObject.GetComponent<Button>();
                component.onClick.Invoke();
            }
        }

        public static void Initialize(Circle_Empires_Editor instance)
        {
            harmony.Patch(AccessTools.Method(typeof(Thing), "ShowTooltip"),null, new HarmonyMethod(thisType, "ShowTooltipPostfix"));
            harmony.Patch(AccessTools.Method(typeof(IntroVideo), "Update"), new HarmonyMethod(thisType, "IntroVideoUpdatePrefix"));
            harmony.Patch(AccessTools.Method(typeof(ManageUI), "Awake"), null,new HarmonyMethod(thisType, "ManageUIAwakePostfix"));
            RuntimeConfig.Circle_Empires_Editor = instance;

            instance.Logger.LogInfo("HarmonyPatches Initialized");
        }
    }

    public static class RuntimeConfig
    {
        public static bool ChangePlayerEnable = false;
        public static bool NoIntroVideo = true;
        public static Circle_Empires_Editor Circle_Empires_Editor = null;

        public static class FontConfig
        {
            public static Font Signika_Bold;
        }

        public static class SpriteConfig
        {
            public static Image BackImage;
            public static Image Button_Brown;
        }
    }

    public class SettingUI : MonoBehaviour
    {
        private void Awake()
        {
            //BackImage
            Misc.Copy(gameObject.AddComponent<Image>(), RuntimeConfig.SpriteConfig.BackImage);

            //Title
            var TitleText = new GameObject("Title").AddComponent<UIComponent.Text>();
            TitleText.text = "Circle Empires Editor Setting";
            TitleText.alignment = TextAnchor.UpperCenter;
            TitleText.font = RuntimeConfig.FontConfig.Signika_Bold;
            TitleText.fontSize = 36;
            TitleText.verticalOverflow = VerticalWrapMode.Overflow;
            TitleText.RectTransform.SetParent(gameObject.transform);
            TitleText.RectTransform.sizeDelta = new Vector2(1000, 40);
            TitleText.RectTransform.anchorMax = new Vector2(0.5f, 1);
            TitleText.RectTransform.anchorMin = TitleText.RectTransform.anchorMax;
            TitleText.RectTransform.anchoredPosition = new Vector2(0, -50);


            //CloseButton
            var CloseButton = new GameObject("CloseButton").AddComponent<UIComponent.Button>();
            CloseButton.RectTransform.SetParent(gameObject.transform);
            CloseButton.RectTransform.sizeDelta = new Vector2(164, 63);
            CloseButton.RectTransform.anchorMax = new Vector2(0.03f, 0.03f);
            CloseButton.RectTransform.anchorMin = new Vector2(0.03f, 0.03f);
            CloseButton.RectTransform.anchoredPosition = new Vector2(0, 0);
            CloseButton.RectTransform.pivot = new Vector2(0, 0);

            CloseButton.SetImage(RuntimeConfig.SpriteConfig.Button_Brown);
            CloseButton.Text.text = "关闭";
            CloseButton.Text.font = RuntimeConfig.FontConfig.Signika_Bold;
            CloseButton.Text.fontSize = 30;

            CloseButton.onClick.AddListener(delegate 
            {
                gameObject.SetActive(false);
            });
        }
    }

}
