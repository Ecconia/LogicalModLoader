using LogicalModLoader.AccessHelper;
using LogicAPI.Client;
using LogicWorld;
using LogicWorld.SharedCode.Modding;

namespace LogicalModLoader.Client
{
	public static class StopLWFromUpdating
	{
		private static object harmonyInstance;
		
		public static void execute()
		{
			var modLoaderLoadMethod = Methods.getPrivate(Types.findInAssembly(typeof(GameStarter), "LogicWorld.GameManager"), "Update");
			var hookMethod = Methods.getPublicStatic(typeof(StopLWFromUpdating), nameof(patchForUpdate));
			harmonyInstance = HarmonyAtRuntime.getHarmonyInstance("LogicalModLoader-StopUpdate");
			HarmonyAtRuntime.patch(harmonyInstance, modLoaderLoadMethod, hookMethod);
		}

		public static void disarm()
		{
			HarmonyAtRuntime.unpatchAll(harmonyInstance);
			harmonyInstance = null;
		}

		public static bool patchForUpdate()
		{
			return false;
		}
	}
}
