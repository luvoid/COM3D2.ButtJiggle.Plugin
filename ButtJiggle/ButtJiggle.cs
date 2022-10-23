using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using MaidStatus;
using PrivateMaidMode;
using RootMotion;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UniverseLib;
using UniverseLib.Input;
using UniverseLib.UI;


// If there are errors in the above using statements, restore the NuGet packages:
// 1. Left-click on the ButtJiggle Project in the Solution Explorer (not ButtJiggle.cs)
// 2. In the pop-up context menu, click on "Manage NuGet Packages..."
// 3. In the top-right corner of the NuGet Package Manager, click "restore"


// You can add references to another BepInEx plugin:
// 1. Left-click on the ButtJiggle Project's references in the Solution Explorer
// 2. Select the "Add Reference..." context menu option.
// 3. Expand the "Assemblies" tab group, and select the "Extensions" tab
// 4. Choose your assemblies then select "Ok"
// 5. Be sure to select each of the added references in the solution explorer,
//    then in the properties window, set "Copy Local" to false.



// This is the major & minor version with an asterisk (*) appended to auto increment numbers.
[assembly: AssemblyVersion(COM3D2.ButtJiggle.PluginInfo.PLUGIN_VERSION + ".*")]

// These two lines tell your plugin to not give a flying fuck about accessing private variables/classes whatever.
// It requires a publicized stubb of the library with those private objects though. 
[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]


namespace COM3D2.ButtJiggle
{
	public static class PluginInfo
	{
		// The name of this assembly.
		public const string PLUGIN_GUID = "COM3D2.ButtJiggle";
		// The name of this plugin.
		public const string PLUGIN_NAME = "Butt Jiggle";
		// The version of this plugin.
		public const string PLUGIN_VERSION = "0.6";
	}
}



namespace COM3D2.ButtJiggle
{
	// This is the metadata set for your plugin.
	[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
	public sealed partial class ButtJiggle : BaseUnityPlugin
	{
		// Static saving of the main instance. (Singleton design pattern)
		// This makes it easier to run stuff like coroutines from static methods or accessing non-static vars.
		public static ButtJiggle Instance { get; private set; }

		// Static property for the logger so you can log from other classes.
		internal static new ManualLogSource Logger => Instance?._Logger;
		private ManualLogSource _Logger => base.Logger;

		internal static ConfigEntry<KeyboardShortcut> UIHotkey;
		internal static ConfigEntry<bool> ButtJiggle_Enabled;
		internal static ConfigEntry<float> ButtJiggle_DefaultSoftness;

		void Awake()
		{
			// Useful for engaging coroutines or accessing non-static variables. Completely optional though.
			Instance = this;

			// Binds the configuration. In other words it sets your ConfigEntry var to your config setup.
			ButtJiggle_Enabled         = Config.Bind("Butt Jiggle", "Enabled"         , true, "Description");
			ButtJiggle_DefaultSoftness = Config.Bind("Butt Jiggle", "DefaultSoftness", 0.5f, "Description");

			// Add the keybind
			KeyboardShortcut hotkey = new KeyboardShortcut(KeyCode.A, KeyCode.LeftControl);
			UIHotkey = Config.Bind("UI", "Toggle", hotkey, "Recomend using Ctrl A for 'Ass'");

			Logger.LogInfo("Patching ButtJiggle");
			Harmony.CreateAndPatchAll(typeof(ButtJiggle));
			
			Logger.LogInfo("Patching TBodyPatch");
			Harmony.CreateAndPatchAll(typeof(TBodyPatch));

			Logger.LogInfo("Patching BoneMorph_Patch");
			Harmony.CreateAndPatchAll(typeof(BoneMorph_Patch));

			Logger.LogInfo("Finished patching");
		}

		void Start()
		{
			Universe_Init();
		}

		private bool m_IsHotkeyHandled = false;
		void Update()
		{
			if (UIHotkey.Value.IsDown() && !m_IsHotkeyHandled)
			{
				ToggleUI();
				m_IsHotkeyHandled = true;
			}
			else if (UIHotkey.Value.IsUp())
			{
				m_IsHotkeyHandled = false;
			}
		}
	}
}
