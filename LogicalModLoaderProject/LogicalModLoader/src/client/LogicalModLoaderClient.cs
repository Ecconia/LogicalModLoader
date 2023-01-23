using JimmysUnityUtilities;
using LogicalModLoader.Client.DamageReduction;
using LogicalModLoader.Client.PrimitiveDebuggingOverlay;
using LogicAPI.Client;

namespace LogicalModLoader.Client
{
	public class LogicalModLoaderClient : ClientMod
	{
		public static PrimitiveDebuggingScreen debug;
		public static string modLoaderID;
		public static LogicalModLoaderFolder modLoaderFolder;

		protected override void Initialize()
		{
			modLoaderID = Manifest.ID;
			
			Logger.Info("Started loading LogicalModLoader!");

			modLoaderFolder = new LogicalModLoaderFolder(Logger, LogicWorldRootFolder.getRootFolder(), true);
			AssemblyLoader.loadAssemblies(modLoaderFolder.getCacheFolder(), Files, true);

			//Stop LogicWorld from loading further mods:
			StopModLoaderFromLoadingMoreMods.execute();
			StopLWFromUpdating.execute();
			BruteForceStopLoading.execute();
			StopSelfDestructionInitialize.execute();

			CoroutineUtility.RunAfterOneFrame(() =>
			{
				debug = new PrimitiveDebuggingScreen();
				debug.append("<i>Successfully hijacked LogicWorld!</i>");
				LogicalModManagerComponent.init();
			});

			Logger.Info("Finished loading LogicalModLoader!");
		}
	}
}
