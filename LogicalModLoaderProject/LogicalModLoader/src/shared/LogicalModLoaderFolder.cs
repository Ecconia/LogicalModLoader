using System;
using System.IO;
using LogicLog;

namespace LogicalModLoader
{
	public class LogicalModLoaderFolder
	{
		private DirectoryInfo modFolder;
		private DirectoryInfo sideFolder;
		private DirectoryInfo cacheFolder;
		private DirectoryInfo modsCacheFolder;

		public LogicalModLoaderFolder(ILogicLogger logger, DirectoryInfo rootFolder, bool isClient)
		{
			modFolder = rootFolder.CreateSubdirectory("LogicalModLoader");
			logger.Info("Using mod folder at: " + modFolder);
			sideFolder = modFolder.CreateSubdirectory(isClient ? "client" : "server");
			cacheFolder = sideFolder.CreateSubdirectory("cache");
			modsCacheFolder = cacheFolder.CreateSubdirectory("mods");
		}

		public DirectoryInfo getCacheForMod(string id, Version version)
		{
			return modsCacheFolder.CreateSubdirectory(id).CreateSubdirectory(version.ToString());
		}

		public DirectoryInfo getCacheFolder()
		{
			return cacheFolder;
		}
	}
}
