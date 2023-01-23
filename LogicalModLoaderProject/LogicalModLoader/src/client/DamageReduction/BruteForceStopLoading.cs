using System;
using System.Reflection;
using CSharpSynth.Banks;
using FancyInput;
using LogicalModLoader.AccessHelper;
using LogicSettings;
using LogicUI;
using LogicWorld;
using LogicWorld.Audio;
using LogicWorld.Input;
using LogicWorld.Modding;
using LogicWorld.Networking;
using LogicWorld.SharedCode.Saving;
using LogicWorld.UI.Thumbnails;

namespace LogicalModLoader.Client
{
	public static class BruteForceStopLoading
	{
		private static object harmonyInstance;

		public static void execute()
		{
			Type modLoader = Types.findInAssembly(typeof(Mods), "LogicWorld.Modding.Loading.ModLoader");
			MethodInfo[] methods = new MethodInfo[]
			{
				//Prevent loading anything, before the mods are loaded fully (via GameManager):
				Methods.getPrivateStatic(modLoader, "LoadLanguages"),
				Methods.getPublic(typeof(SettingsManager), "RegisterAllSettings"),
				Methods.getPublicStatic(typeof(InputManager), "EnsureTriggersAndContextsAreRegistered"),
				Methods.getPublicStatic(typeof(Input), "EnsureTriggersAndContextsAreRegistered"),
				Methods.getPublicStatic(typeof(CjkFontStyleManager), "Initialize"),
				Methods.getPublicStatic(typeof(BindingsManager), "ReloadBindingsFromFile"),
				Methods.getPrivateStatic(typeof(GameNetwork), "Initialize"),
				Methods.getPublicStatic(typeof(InstrumentBank), "Initialize"),
				Methods.getPrivateStatic(typeof(SoundEffectDatabase), "ReloadSoundDatabase"),
				Methods.getPublicStatic(typeof(BackupUtility), "DeleteBackupsOfDeletedItemsIfExpired"),
				//Prevent creation of thumbnails (while components are not loaded):
				Methods.getPrivate(typeof(ItemThumbnails), "Start"),
				Methods.getPrivate(typeof(ItemThumbnails), "Update"),
				//Prevent the game from entering the main menu at some arbitrary point in time:
				Methods.getPrivate(typeof(GameStarter), "Start"),
			};

			harmonyInstance = HarmonyAtRuntime.getHarmonyInstance("LogicalModLoader-BruteForceStopLoading");
			var hookMethod = Methods.getPublicStatic(typeof(BruteForceStopLoading), nameof(patchForUpdate));
			foreach(var method in methods)
			{
				HarmonyAtRuntime.patch(harmonyInstance, method, hookMethod);
			}
		}
		
		public static bool patchForUpdate()
		{
			return false;
		}
		
		public static void disarm()
		{
			HarmonyAtRuntime.unpatchAll(harmonyInstance);
			harmonyInstance = null;
		}
	}
}
