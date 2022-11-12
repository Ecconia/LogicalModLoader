using LogicAPI.Client;

namespace LogicalModLoader.Client
{
	public class LogicalModLoaderClient : ClientMod
	{
		protected override void Initialize()
		{
			Logger.Info("Started loading LogicalModLoader!");
			
			var modLoaderFolder = new LogicalModLoaderFolder(Logger, LogicWorldRootFolder.getRootFolder(), true);
			AssemblyLoader.loadAssemblies(modLoaderFolder.getCacheFolder(), Files, true);
			
			// StopModLoaderFromLoadingMoreMods.execute();
			
			Logger.Info("Finished loading LogicalModLoader!");
		}
	}
}
