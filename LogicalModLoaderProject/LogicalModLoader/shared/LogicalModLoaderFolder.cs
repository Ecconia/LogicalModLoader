using System.IO;
using LogicLog;

namespace LogicalModLoader
{
	public class LogicalModLoaderFolder
	{
		private DirectoryInfo modFolder;
		private DirectoryInfo sideFolder;
		private DirectoryInfo cacheFolder;

		public LogicalModLoaderFolder(ILogicLogger logger, DirectoryInfo rootFolder, bool isClient)
		{
			modFolder = rootFolder.CreateSubdirectory("LogicalModLoader");
			logger.Info("Using mod folder at: " + modFolder);
			sideFolder = modFolder.CreateSubdirectory(isClient ? "client" : "server");
			cacheFolder = sideFolder.CreateSubdirectory("cache");
		}

		public DirectoryInfo getCacheFolder()
		{
			return cacheFolder;
		}
	}
}
