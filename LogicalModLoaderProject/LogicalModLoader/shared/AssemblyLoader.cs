using System.IO;
using System.Reflection;
using LogicalModLoader.Util;
using LogicAPI;

namespace LogicalModLoader
{
	public static class AssemblyLoader
	{
		public static void loadAssemblies(DirectoryInfo cacheFolder, IModFiles files, bool isClient)
		{
			foreach(ModFile modFile in files.EnumerateFiles())
			{
				if(
					".dll".Equals(modFile.Extension)
					&& (
						modFile.Path.StartsWith("assembly/shared/")
						|| (isClient && modFile.Path.StartsWith("assembly/client/"))
						|| (!isClient && modFile.Path.StartsWith("assembly/server/"))
					)
				)
				{
					load(cacheFolder, modFile);
				}
			}
		}
		
		private static void load(DirectoryInfo cacheFolder, ModFile modFile)
		{
			var fileHash = Hashing.hashModFile(modFile);
			var cachedFilePath = Path.Combine(cacheFolder.FullName, modFile.FileName + "-" + fileHash + modFile.Extension);
			
			//Always overwrite the file:
			using(Stream outputStream = File.Create(cachedFilePath))
			{
				using(Stream inputStream = modFile.OpenRead())
				{
					inputStream.CopyTo(outputStream);
				}
			}
			
			//Load the assembly:
			Assembly.LoadFrom(cachedFilePath);
		}
	}
}
