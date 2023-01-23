using LogicalModLoader.AccessHelper;
using LogicAPI;
using LogicAPI.Client;
using LogicWorld.SharedCode.Modding;

namespace LogicalModLoader.Client.DamageReduction
{
	public static class StopModLoaderFromLoadingMoreMods
	{
		private static object harmonyInstance;
		public static void execute()
		{
			var modLoaderLoadMethod = Methods.getPublicStatic(typeof(ModLoader), "LoadMod").MakeGenericMethod(typeof(ClientMod));
			var hookMethod = Methods.getPublicStatic(typeof(StopModLoaderFromLoadingMoreMods), nameof(patchForLoadMod));
			harmonyInstance = HarmonyAtRuntime.getHarmonyInstance("LogicalModLoader-StopModLoader");
			HarmonyAtRuntime.patch(harmonyInstance, modLoaderLoadMethod, hookMethod);
		}

		public static void disarm()
		{
			HarmonyAtRuntime.unpatchAll(harmonyInstance);
			harmonyInstance = null;
		}

		public static bool patchForLoadMod(ref (int, int, bool) __result)
		{
			__result = (0, 0, true);
			return false;
		}

		public static bool patchForLoadMod2(ref (int, int, bool) __result, MetaMod metaMod)
		{
			if(metaMod.Manifest.ID.Equals("MHG"))
			{
				return true;
			}
			__result = (0, 0, true);
			return false;
		}
	}
}
