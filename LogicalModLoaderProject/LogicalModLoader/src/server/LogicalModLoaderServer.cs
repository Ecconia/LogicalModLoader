using LogicalModLoader.Server;
using LogicAPI.Server;

namespace LogicalModLoader.server
{
	public class LogicalModLoaderServer : ServerMod
	{
		protected override void Initialize()
		{
			Logger.Info("Started loading LogicalModLoader!");
			
			var modLoaderFolder = new LogicalModLoaderFolder(Logger, LogicWorldRootFolder.getRootFolder(), false);
			AssemblyLoader.loadAssemblies(modLoaderFolder.getCacheFolder(), Files, false);
			
			StopModLoaderFromLoadingMoreModsServer.execute();
			
			Logger.Info("Finished loading LogicalModLoader!");
		}
	}
}
