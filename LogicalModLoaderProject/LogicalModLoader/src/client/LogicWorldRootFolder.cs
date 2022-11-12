using System.IO;
using UnityEngine;

namespace LogicalModLoader.Client
{
	public static class LogicWorldRootFolder
	{
		public static DirectoryInfo getRootFolder()
		{
			return new DirectoryInfo(Application.dataPath).Parent;
		}
	}
}
