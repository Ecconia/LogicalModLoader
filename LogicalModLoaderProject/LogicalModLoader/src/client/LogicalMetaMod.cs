using LogicAPI.Modding;
using LogicWorld.SharedCode.Modding;

namespace LogicalModLoader.Client
{
	public abstract class LogicalMetaMod
	{
		public FolderModFiles modFiles { get; }

		protected LogicalMetaMod(FolderModFiles modFiles)
		{
			this.modFiles = modFiles;
		}
	}

	public class LogicalMetaModSimple : LogicalMetaMod
	{
		private readonly ModManifest manifest;

		public LogicalMetaModSimple(FolderModFiles modFiles, ModManifest manifest) : base(modFiles)
		{
			this.manifest = manifest;
		}

		public ModManifest getModManifest()
		{
			return manifest;
		}
	}
}
