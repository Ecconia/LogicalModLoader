using LogicalModLoader.AccessHelper;
using LogicWorld.Server;
using LogicWorld.Server.Managers;

namespace LogicalModLoader.Server
{
	public static class StopModLoaderFromLoadingMoreModsServer
	{
		public static void execute()
		{
			var harmonyInstance = HarmonyAtRuntime.getHarmonyInstance("LogicalModLoader-StopModLoader");
			var modLoader = Program.Get<IModManager>();
			var method = Methods.getPrivate(modLoader, "LoadMetaMod");
			var hookMethod = Methods.getPublicStatic(typeof(StopModLoaderFromLoadingMoreModsServer), nameof(patchForLoadMod));
			HarmonyAtRuntime.patch(harmonyInstance, method, hookMethod);
		}
		
		public static bool patchForLoadMod()
		{
			return false;
		}
	}
}
