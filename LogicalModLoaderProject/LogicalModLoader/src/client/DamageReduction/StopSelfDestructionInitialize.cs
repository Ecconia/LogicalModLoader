using System;
using System.Reflection;
using LogicalModLoader.AccessHelper;
using LogicSettings;
using UnityEngine;

namespace LogicalModLoader.Client.DamageReduction
{
	public static class StopSelfDestructionInitialize
	{
		private static object harmonyInstance;
		private static MethodInfo initializeMethod;
		private static SettingsMenu instance;

		public static void execute()
		{
			initializeMethod = Methods.getPrivate(typeof(SettingsMenu), "LogicWorld.UnityHacksAndExtensions.IInitializable.Initialize");
			harmonyInstance = HarmonyAtRuntime.getHarmonyInstance("LogicalModLoader-StopSelfDestructionInitialize");
			var hookMethod = Methods.getPublicStatic(typeof(StopSelfDestructionInitialize), nameof(patchForUpdate));
			HarmonyAtRuntime.patch(harmonyInstance, initializeMethod, hookMethod);
		}
		
		public static bool patchForUpdate(SettingsMenu __instance)
		{
			if(instance != null)
			{
				throw new Exception("[LogicalModLoader] Whoops, the patch to stop initializing SettingsMenu was called twice?!");
			}
			instance = __instance;
			return false;
		}

		public static void disarm()
		{
			HarmonyAtRuntime.unpatchAll(harmonyInstance);
			harmonyInstance = null;
		}

		public static void fix()
		{
			if(instance == null)
			{
				//TODO: Throw big fat warning, this should not happen.
				return;
			}
			initializeMethod.Invoke(instance, null);
		}
	}
}
