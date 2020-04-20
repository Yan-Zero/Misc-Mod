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

namespace Circle_Empires_Editor
{

	[BepInPlugin("Yan.CircleEmpiresEditor", "Circle Empires Editor", "0.3.0")]
	public class Circle_Empires_Editor : BaseUnityPlugin
	{
        private static GameObject manager = null;
        public static ManageUI manageUI = null;
        public static General general = null;

        public ConfigEntry<KeyboardShortcut> Hotkey_UpLevel { get; private set; }
        public ConfigEntry<KeyboardShortcut> Hotkey_SetSpeed { get; private set; }
        public ConfigEntry<KeyboardShortcut> Hotkey_ChangePlayer { get; private set; }

        public Color Unfreeze;

        private int timeChangePlayer = 100;

        public bool Initialize()
        {
            try
            {
                if (manager == null)
                {
                    manager = GameObject.FindGameObjectWithTag("Manager");
                    manageUI = manager.GetComponent<ManageUI>();
                    Unfreeze = ManageUI.HexToColor("FF3300FF");
                    general = manager.GetComponent<General>();
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
            if (!Initialize())
                return;

            if (manageUI.selectedThings.Count >= 1)
            {
                for(int i = 0; i < manageUI.selectedThings.Count;)
                {
                    if (manageUI.selectedThings[i].GainXp(100000000) == 0)
                        i++;
                }
            }
            else
                Logger.LogWarning("Please Select Thing");

        }

        private void ShowUnfreezeVisual(Thing thing)
        {
            if (thing.spriteRenderer != null)
                thing.spriteRenderer.color = Unfreeze;
            if (thing.droppableFreezeParticles != null)
                thing.droppableFreezeParticles.gameObject.SetActive(value: true);
            else if (thing.visualGroup != null)
            {
                GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(position: new Vector3((float)thing.currentPos.x / 1000f, (float)thing.currentPos.y / 1000f * (float)thing.general.angleY / 1000f, 0f), original: thing.frozenParticlesPrefab, rotation: Quaternion.identity);
                gameObject.transform.SetParent(thing.visualGroup.transform);
                thing.droppableFreezeParticles = gameObject.GetComponent<DroppableParticles>();
                thing.droppableFreezeParticles.Init(base.gameObject);
            }
        }

        private void SetSpeed()
        {
            Logger.Log(LogLevel.Info, "SetSpeed");
            if (!Initialize())
                return;

            if (manageUI.selectedThings.Count >= 1)
            {
                foreach(var i in manageUI.selectedThings)
                {
                    ShowUnfreezeVisual(i);
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
            if (Hotkey_ChangePlayer.Value.IsDown())
            {
                if (!Initialize())
                    return;
                RuntimeConfig.ChangePlayerEnable = !RuntimeConfig.ChangePlayerEnable;
                timeChangePlayer = 0;
                Logger.Log(LogLevel.Info, "ChangePlayer " + ( RuntimeConfig.ChangePlayerEnable ? "Enable" : "Disable"));
            }
            if(timeChangePlayer < 100)
            {
                manageUI.ShowToolTip(10, Screen.height - 10, "ChangePlayer " + (RuntimeConfig.ChangePlayerEnable ? "Enable" : "Disable"));
                timeChangePlayer++;
            }
        }

        private void Awake()
        {
            Hotkey_UpLevel = Config.Bind("Cheat", "Apply UpLevel Cheat", new KeyboardShortcut(KeyCode.F12, new KeyCode[] { KeyCode.LeftControl }));
            Hotkey_SetSpeed = Config.Bind("Cheat", "Apply SetSpeed Cheat", new KeyboardShortcut(KeyCode.F9, new KeyCode[] { KeyCode.LeftControl }));
            Hotkey_ChangePlayer = Config.Bind("Cheat", "Apply ChangePlayer Cheat", new KeyboardShortcut(KeyCode.F8, new KeyCode[] { KeyCode.LeftControl }));

            HarmonyPatches.Initialize();
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

        public static void Initialize()
        {
            harmony.Patch(AccessTools.Method(typeof(Thing), "ShowTooltip"),null, new HarmonyMethod(thisType, "ShowTooltipPostfix"));
        }
    }

    public static class RuntimeConfig
    {
        public static bool ChangePlayerEnable = false;
    }

}
