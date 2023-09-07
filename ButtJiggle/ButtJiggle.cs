using BepInEx;
using BepInEx.Configuration;
using BepInEx.Configuration.Json;
using BepInEx.Logging;
using HarmonyLib;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using UnityEngine;
using UnityEngine.Events;
using CM3D2.UGUI;



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
[assembly: AssemblyFileVersion(COM3D2.ButtJiggle.PluginInfo.PLUGIN_VERSION)]

// These two lines tell your plugin to not give a flying flip about accessing private variables/classes whatever.
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
		public const string PLUGIN_VERSION = "0.13";
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


		internal static ConfigEntry<double> ButtJiggle_ConfigVersion;

		internal static ConfigEntry<bool> ButtJiggle_Enabled;
		internal static ConfigEntry<float> ButtJiggle_DefaultSoftness_Hip;
		internal static ConfigEntry<float> ButtJiggle_DefaultSoftness_Pelvis;

		//internal static ConfigEntry<bool> Experimental_PelvisEnabled;

		internal static ConfigEntry<bool> GlobalOverride_Enabled;
		internal static ConfigEntry<string> GlobalOverride_Json;
		internal static ConfigEntryJson<MaidJiggleOverride> GlobalOverride_Settings;

		public UnityEvent OnGlobalOverrideUpdated;

		void Awake()
		{
			// Useful for engaging coroutines or accessing non-static variables. Completely optional though.
			Instance = this;

			// Binds the configuration. In other words it sets your ConfigEntry var to your config setup.
			ButtJiggle_ConfigVersion = Config.Bind("ButtJiggle", "ConfigVersion", 0.0, new ConfigDescription(
				"Do not change this",
				null,
				new ConfigurationManagerAttributes()
				{
					IsAdvanced = true,
					ReadOnly = true,
				}
			));

			ButtJiggle_Enabled = Config.Bind("ButtJiggle", "Enabled", true);
			ButtJiggle_DefaultSoftness_Hip    = Config.Bind("ButtJiggle", "DefaultSoftness.Hip"   , MaidJiggleOverride.Default.HipOverride.Softness.Value);
			ButtJiggle_DefaultSoftness_Pelvis = Config.Bind("ButtJiggle", "DefaultSoftness.Pelvis", MaidJiggleOverride.Default.PelvisOverride.Softness.Value);
			if (ButtJiggle_ConfigVersion.Value < 0.11)
			{
				ButtJiggle_DefaultSoftness_Pelvis.Value = MaidJiggleOverride.Default.PelvisOverride.Softness.Value;
			}

			//Experimental_PelvisEnabled = Config.Bind("Experimental", "PelvisEnabled", false);


			ConfigBindGlobalOverride();

			// Add the keybind
			KeyboardShortcut hotkey = new KeyboardShortcut(KeyCode.A, KeyCode.LeftControl);
			UIHotkey = Config.Bind("UI", "Toggle", hotkey, "Recommend using Ctrl A for 'Ass'");

			ButtJiggle_ConfigVersion.Value = double.Parse(PluginInfo.PLUGIN_VERSION);

			Logger.LogDebug("Patching ButtJiggle");
			Harmony.CreateAndPatchAll(typeof(ButtJiggle));
			
			Logger.LogDebug("Patching TBodyPatch");
			Harmony.CreateAndPatchAll(typeof(TBodyPatch));

			Logger.LogDebug("Patching BoneMorph_Patch");
			Harmony.CreateAndPatchAll(typeof(BoneMorph_Patch));

			Logger.LogDebug("Finished patching");
		}

		private void ConfigBindGlobalOverride()
		{
			if (GlobalOverride_Enabled != null) return;

			string defaultJson = SerializeGlobalOverride();

			GlobalOverride_Enabled  = Config.Bind("GlobalOverride", "Enabled", false      , "Enable global override of jiggle settings");
			//GlobalOverride_Json     = Config.Bind("Global Override", "Json"   , defaultJson, "The jiggle settings in JSON format"       );
			GlobalOverride_Settings = Config.BindJson("GlobalOverride", "Settings", MaidJiggleOverride.Default, "The settings used for the global override");

			if (ButtJiggle_ConfigVersion.Value < 0.11)
			{
				GlobalOverride_Settings.Value = MaidJiggleOverride.Default;
			}

			JiggleBoneHelper.UseGlobalOverride = GlobalOverride_Enabled.Value;
			JiggleBoneHelper.GlobalOverride = GlobalOverride_Settings.Value;

			GlobalOverride_Enabled.SettingChanged += delegate (object sender, EventArgs eventArgs)
			{
				JiggleBoneHelper.UseGlobalOverride = GlobalOverride_Enabled.Value;
				OnGlobalOverrideUpdated.Invoke();
			};
			GlobalOverride_Settings.SettingChanged += delegate (object sender, EventArgs eventArgs)
			{
				JiggleBoneHelper.GlobalOverride = GlobalOverride_Settings.Value;
				OnGlobalOverrideUpdated.Invoke();
			};
		}

		public static void ConfigSaveGlobalOverride()
		{
			Logger.LogDebug("ConfigSaveGlobalOverride");
			GlobalOverride_Enabled .Value = JiggleBoneHelper.UseGlobalOverride;
			//GlobalOverride_Json    .Value = SerializeGlobalOverride();
			GlobalOverride_Settings.Value = JiggleBoneHelper.GlobalOverride;
		}

		private static string SerializeGlobalOverride()
		{
			JsonSerializer serializer = new JsonSerializer();
			StringWriter writer = new StringWriter();
			serializer.Serialize(writer, JiggleBoneHelper.GlobalOverride);
			return writer.ToString();
		}
	}
}
