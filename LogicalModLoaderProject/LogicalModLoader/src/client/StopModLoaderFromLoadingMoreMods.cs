using LogicalModLoader.AccessHelper;
using LogicAPI.Client;
using LogicWorld.SharedCode.Modding;

namespace LogicalModLoader.Client
{
	public static class StopModLoaderFromLoadingMoreMods
	{
		public static void execute()
		{
			var modLoaderLoadMethod = Methods.getPublicStatic(typeof(ModLoader), "LoadMod").MakeGenericMethod(typeof(ClientMod));
			var hookMethod = Methods.getPublicStatic(typeof(StopModLoaderFromLoadingMoreMods), nameof(patchForLoadMod));
			var harmonyInstance = HarmonyAtRuntime.getHarmonyInstance("LogicalModLoader-StopModLoader");
			HarmonyAtRuntime.patch(harmonyInstance, modLoaderLoadMethod, hookMethod);
		}

		public static bool patchForLoadMod(ref (int, int, bool) __result)
		{
			__result = (0, 0, true);
			return false;
		}
	}
}
