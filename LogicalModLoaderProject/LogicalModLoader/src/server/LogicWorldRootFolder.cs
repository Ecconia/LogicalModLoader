using System;
using System.IO;

namespace LogicalModLoader.Server
{
	public static class LogicWorldRootFolder
	{
		public static DirectoryInfo getRootFolder()
		{
			return new DirectoryInfo(AppContext.BaseDirectory).Parent;
		}
	}
}
