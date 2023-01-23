using LICC;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using CSharpSynth.Banks;
using FancyInput;
using GameDataAccess;
using JimmysUnityUtilities;
using LogicalModLoader.AccessHelper;
using LogicalModLoader.Client.DamageReduction;
using LogicAPI;
using LogicAPI.Client;
using LogicAPI.Modding;
using LogicLocalization;
using LogicLog;
using LogicSettings;
using LogicUI;
using LogicWorld;
using LogicWorld.Audio;
using LogicWorld.Input;
using LogicWorld.Modding;
using LogicWorld.Modding.Assets;
using LogicWorld.Modding.Assets._originalassets.scripts.modding;
using LogicWorld.Networking;
using LogicWorld.SharedCode.Components;
using LogicWorld.SharedCode.Modding;
using LogicWorld.SharedCode.Saving;
using LogicWorld.UI.Thumbnails;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;
using Shared_Code.Code.Networking;
using SUCC;
using SUCC.Abstractions;
using SUCC.MemoryFiles;
using UnityEngine;
using UnityEngine.SceneManagement;
using Types = LogicalModLoader.AccessHelper.Types;

namespace LogicalModLoader.Client
{
	public class LogicalModManagerComponent : MonoBehaviour
	{
		public static LogicalModManagerComponent init()
		{
			GameObject gameObject = new GameObject("LogicalModLoader");
			return gameObject.AddComponent<LogicalModManagerComponent>();
		}

		private void Awake()
		{
			LogicalModLoaderClient.debug.append("Awake");
		}

		private void Start()
		{
			LogicalModLoaderClient.debug.append("start");
		}

		private bool doStuff = true;

		private void Update()
		{
			if(doStuff)
			{
				doStuff = false;

				//At this point, all the initialize code has finished and is no longer doing anything.
				// Lets undo the force breaking of it again:
				StopModLoaderFromLoadingMoreMods.disarm(); //Does no longer matter anymore, so undo.
				BruteForceStopLoading.disarm(); //All initialization code, that got disrupted by this, is no longer being called.
				StopSelfDestructionInitialize.disarm();
				
				//Do stuff:

				LogicalModLoaderClient.debug.append("compile");
				CoroutineUtility.RunAfterOneFrame(() =>
				{
					CoroutineUtility.RunAfterOneFrame(() =>
					{
						doActualStuff();
						LogicalModLoaderClient.debug.append("done!");
					});
				});
			}
		}

		private void doActualStuff()
		{
			//Find mods:

			HashSet<string> failedToLoadModIDs = new HashSet<string>();
			List<LogicalMetaModSimple> metaMods = new List<LogicalMetaModSimple>();
			{
				HashSet<string> loadedModIDs = new HashSet<string>();
				//As LW is not supporting *.lwmod files properly, we won't support them at all.
				foreach(var potentialModFolder in GameData.GetAllSubfolders())
				{
					//Skip this folder, if there is no ignore file:
					if(File.Exists(Path.Combine(potentialModFolder.FullName, "ignore")))
					{
						Debug.Log("Ignoring mod folder: " + potentialModFolder.Name);
						continue;
					}
					//Skip this folder, if there is no manifest file:
					FolderModFiles files = new FolderModFiles(potentialModFolder.FullName);
					if(!files.Exists("manifest.succ"))
					{
						Debug.Log("Skipping folder: " + potentialModFolder.Name);
						continue;
					}
					MemoryDataFile succManifestFile;
					try
					{
						succManifestFile = new MemoryDataFile(files.GetFile("manifest.succ").ReadAllText());
					}
					catch(Exception e)
					{
						Debug.Log("Skipping mod folder '" + potentialModFolder.Name + "' as the manifest file cannot be loaded and parsed: " + e.Message);
						continue;
					}
					ModManifest modManifest = succManifestFile.GetAsObject<ModManifest>();
					if(string.IsNullOrWhiteSpace(modManifest.ID) || !Regex.IsMatch(modManifest.ID, "[a-zA-Z0-9._-]+"))
					{
						Debug.Log("Skipping mod folder '" + potentialModFolder.Name + "' as the mod-id in the manifest is not gud enuf. Must match: '[a-zA-Z0-9._-]+'.");
						continue;
					}
					if(ModLoader.LoadedMods.ContainsKey(modManifest.ID))
					{
						Debug.Log("Skipping mod folder '" + potentialModFolder.Name + "' as the mod '" + modManifest.ID + "' was already loaded by LW.");
						continue;
					}
					if(modManifest.ID.Equals(LogicalModLoaderClient.modLoaderID))
					{
						Debug.Log("Skipping mod folder '" + potentialModFolder.Name + "' as it is LogicalModLoader (this mod) with ID: '" + modManifest.ID + "'");
						continue;
					}
					if(modManifest.Priority < -100 || modManifest.Priority > 100)
					{
						failedToLoadModIDs.Add(modManifest.ID);
						Debug.Log("Skipping mod folder '" + potentialModFolder.Name + "' as the priority is out of range, it must be a value from -100 to 100.");
						continue;
					}
					if(loadedModIDs.Contains(modManifest.ID))
					{
						//TODO: Print the folder of that mod that loaded the ID first!
						Debug.Log("Skipping mod folder '" + potentialModFolder.Name + "' as a mod with the same ModID has already been loaded: '" + modManifest.ID + "'");
						continue;
					}
					metaMods.Add(new LogicalMetaModSimple(files, modManifest));
				}
			}

			//Order mods:
			//TODO: Change to dependency graph system. This is temporary for now, until custom manifests exist.
			metaMods.Sort(((a, b) => b.getModManifest().Priority - a.getModManifest().Priority));

			//Compile mods:
			DispatcherAdapter dispatcher = new DispatcherAdapter();
			foreach(var metaMod in metaMods)
			{
				Debug.Log("Loading mod: " + metaMod.getModManifest().ID);
				MetaMod fakeMod = (MetaMod) Activator.CreateInstance(typeof(MetaMod), Bindings.privateInst, null, new object[]
				{
					metaMod.modFiles,
					metaMod.getModManifest(),
					false,
					ModFormat.Folder,
				}, null, null);
				
				//If server/client/shared load C#:
				//Compile all code (store to cache)
				string assemblyPath = compile(metaMod);
				if(assemblyPath != null) //If it is 'null' there was no code to compile.
				{
					//Load DLL
					Assembly assembly = Assembly.LoadFile(assemblyPath);
					fakeMod.GetType().GetProperty("CodeAssembly", Bindings.publicInst).SetValue(fakeMod, assembly);
					//Create instance - IF NEEDED!
					ClientMod instance = null;
					{
						Type[] array = assembly.GetTypes().Where(typeof(ClientMod).IsAssignableFrom).ToArray();
						if(array.Length > 1)
						{
							//More than one mod entry point is not allowed.
							//TODO: Error.
							throw new Exception("Yeah stop here, mod wants more than one entry point.");
						}
						if(array.Length == 1)
						{
							var type = array[0];
							//Instantiate mod class!
							var constructor = type.GetConstructor(Type.EmptyTypes);
							if(constructor == null)
							{
								throw new Exception("Constructor for mod entry point is missing.");
							}
							instance = (ClientMod) Activator.CreateInstance(type);
							//TODO: Set fake Manifest.
							typeof(BaseMod).GetProperty("Files", Bindings.privateInst).SetValue(instance, metaMod.modFiles);
							//Create logger/factory
							var loggerFactory = new LogicLoggerFactory(metaMod.getModManifest().Name);
							typeof(BaseMod).GetProperty("LoggerFactory", Bindings.privateInst).SetValue(instance, loggerFactory);
							typeof(BaseMod).GetProperty("Logger", Bindings.privateInst).SetValue(instance, loggerFactory.CreateLogger());
							//Specific client code:
							typeof(ClientMod).GetProperty("Dispatcher", Bindings.privateInst).SetValue(instance, dispatcher);
							typeof(ClientMod).GetProperty("Assets", Bindings.privateInst).SetValue(instance, new AssetLoader(metaMod.modFiles));
							
							fakeMod.GetType().GetProperty("ModInstance", Bindings.publicInst).SetValue(fakeMod, instance);
						}
					}
					
					//Load packets
					PacketManager.LoadPacketsIn(assembly);
					//Load commands
					FancyPantsConsole.Console.CommandRegistry.RegisterCommandsIn(assembly);
					//Register events
					//TODO: Eventually use the event system... But its super unfinished anyway and no modder knows it exists.
					// One is better off, just hooking into the build requests with Harmony.
					// EventManager.RegisterHandlersIn(metaMod);
					//"before initialize":
					{
						//Set dispatcher
						//Done above.
						//Set assets
						//Done above.
						
						//"Load proxy"
						//TODO: No clue what for - pointless!
						//"watch components"
						//TODO: Later!
					}
					//Initialize mod instance - if existing
					if(instance != null)
					{
						Methods.getPrivate(typeof(BaseMod), "OnInitialize").Invoke(instance, null);
					}
				}
				
				//Load components:
				if(metaMod.modFiles.ExistsFolder("components/"))
				{
					foreach (ModFile file1 in metaMod.modFiles.EnumerateFiles().Where((Func<ModFile, bool>) (o => o.Path.StartsWith("components/"))))
					{
						ReadableDataFile file2 = file1.ReadAsSUCC();
						int amount = ComponentRegistry.LoadComponentsFile(file2, fakeMod).Length;
						Debug.Log("=> Loaded " + amount + " components.");
					}
				}

				//Add to list of all mods?
				//TODO: Add mod to this list.
				//Add mod files to mod-assets!
				ModAssets.AddFiles(metaMod.modFiles);
				Mods.All.Add(fakeMod);
			}

			//Do post compilation:
			// Load languages
			foreach (KeyValuePair<string, DistributedData> folderSortedSuccData in ModAssets.GetFolderSortedSuccDatas("languages"))
			{
				TextLocalizer.AddLanguage(folderSortedSuccData.Key, folderSortedSuccData.Value);
			}
			// Register all settings
			SettingsManager.Instance.RegisterAllSettings(ModAssets.GetDistributedSuccData("settings/*settingsdata*"));

			//Do whatever was left:
			InputManager.EnsureTriggersAndContextsAreRegistered();
			LogicUI.Input.EnsureTriggersAndContextsAreRegistered();
			CjkFontStyleManager.Initialize();
			BindingsManager.ReloadBindingsFromFile();
			Methods.getPrivateStatic(typeof(GameNetwork), "Initialize").Invoke(null, null);
			InstrumentBank.Initialize();
			var gameManagerType = Types.findInAssembly(typeof(GameStarter), "LogicWorld.GameManager");
			BackupUtility.DeleteBackupsOfDeletedItemsIfExpired((int) gameManagerType.GetProperty("DaysToKeepBackupsOfDeletedItems", Bindings.privateStatic).GetValue(null));
			//Thumbnails:
			{
				var thumbnailRenderer = GameObject.Find("ComponentRenderer");
				if(thumbnailRenderer == null)
				{
					throw new Exception("It seems that the thumbnail renderer was renamed - at least it is not found!");
				}
				ItemThumbnails renderer = thumbnailRenderer.GetComponent<ItemThumbnails>();
				if(renderer == null)
				{
					throw new Exception("The ItemThumbnails component is missing on the renderer game object.");
				}
				Methods.getPrivate(renderer.GetType(), "Start").Invoke(renderer, null);
				//The rest should happen automatically...
			}
			
			//### run everything that goes ONE FRAME LATER: ####
			
			//Initialize the SettingsMenu, as that had to be delayed:
			StopSelfDestructionInitialize.fix();
			
			//Coroutine means, that this is executed after everything else:
			CoroutineUtility.RunAfterOneFrame(() =>
			{
				Methods.getPrivateStatic(typeof(SoundEffectDatabase), "ReloadSoundDatabase").Invoke(null, null);
			});
			
			//Re-Enable game loop:
			StopLWFromUpdating.disarm();
			
			//Switch scene:
			SceneAndNetworkManager.GoToMainMenu();
		}

		private string compile(LogicalMetaModSimple metaMod)
		{
			//Collect the source files:
			//TODO: Add side preprocessor prefix:
			CSharpParseOptions options = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp7_3).WithPreprocessorSymbols("RELEASE", "LW");
			IEnumerable<SyntaxTree> sources =
				metaMod
					.modFiles.EnumerateFiles().Where((Func<ModFile, bool>) (o =>
					{
						if(o.Extension != ".cs")
							return false;
						return o.Path.StartsWith(ModPaths.SharedFolder) || o.Path.StartsWith(ModPaths.ClientFolder);
					}))
					.Select(
						file => SourceText.From(
							file.OpenRead(),
							Encoding.UTF8,
							SourceHashAlgorithm.Sha1,
							false,
							true //This is different from LW!
						))
					.Select(source => SyntaxFactory.ParseSyntaxTree(source, options));
			if(!sources.Any())
			{
				Debug.Log("=> No code files!");
				return null;
			}

			// StringBuilder debug = new StringBuilder();
			// AppDomain.CurrentDomain.GetAssemblies().ForEach(assembly =>
			// {
			// 	debug.Append('\t').Append(assembly.Location).Append('\n');
			// });
			// Debug.Log("Assemblies:\n" + debug);
			IEnumerable<MetadataReference> references =
				AppDomain
					.CurrentDomain.GetAssemblies()
					.Where(assembly =>
					{
						return !assembly.IsDynamic && !string.IsNullOrEmpty(assembly.Location);
					})
					.Select(assembly =>
					{
						return MetadataReference.CreateFromFile(assembly.Location);
					});
			
			CSharpCompilation compilation = CSharpCompilation.Create(
				metaMod.getModManifest().ID,
				sources,
				references,
				new CSharpCompilationOptions(
					OutputKind.DynamicallyLinkedLibrary,
					false,
					null,
					null,
					null,
					null,
					OptimizationLevel.Release,
					false,
					false,
					null,
					null,
					new ImmutableArray<byte>(),
					new bool?(),
					Platform.AnyCpu,
					ReportDiagnostic.Default,
					4,
					null,
					true,
					false,
					null,
					null,
					null,
					null,
					null,
					false,
					MetadataImportOptions.Public
				)
			);

			using(MemoryStream assemblyStream = new MemoryStream())
			{
				using(MemoryStream pdbStream = new MemoryStream())
				{
					EmitResult emitResult = compilation.Emit(
						assemblyStream,
						pdbStream,
						null,
						null,
						null,
						null,
						null,
						null,
						null,
						null,
						new CancellationToken()
					);
					if(emitResult.Success)
					{
						assemblyStream.Position = 0;
						pdbStream.Position = 0;
						
						//Save to files:
						Debug.Log("Should be saved to: " + compilation.AssemblyName);
						var dest = LogicalModLoaderClient.modLoaderFolder.getCacheForMod(metaMod.getModManifest().ID, metaMod.getModManifest().Version);
						var assemblyPath = Path.GetFullPath(Path.Combine(dest.FullName, compilation.AssemblyName + ".dll"));
						var outStream = File.OpenWrite(assemblyPath);
						assemblyStream.CopyTo(outStream);
						outStream.Close();
						outStream = File.OpenWrite(Path.GetFullPath(Path.Combine(dest.FullName, compilation.AssemblyName + ".pdb")));
						pdbStream.CopyTo(outStream);
						outStream.Close();
						return assemblyPath;
					}
					else
					{
						Debug.Log("Failed to compile:");
						foreach(Diagnostic diagnostic in emitResult.Diagnostics)
						{
							string text = diagnostic.ToString();
							if(!text.Contains("Assuming assembly reference 'mscorlib"))
							{
								Debug.Log(diagnostic);
							}
						}
						throw new Exception("Just stop execution here.");
					}
				}
			}
		}
	}
}
