using System;
using System.Reflection;
using LogicalModLoader.AccessHelper;
using Types = LogicalModLoader.AccessHelper.Types;

namespace LogicalModLoader
{
	public static class HarmonyAtRuntime
	{
		private static Type harmonyType;
		private static Type harmonyMethodType;
		private static MethodInfo harmonyPatchMethod;
		
		private static void init()
		{
			if(harmonyPatchMethod != null)
			{
				//We already got things, nothing to initialize anymore.
				return;
			}
			Assembly harmonyAssembly = Assemblies.findAssemblyWithName("0Harmony");
			harmonyType = Types.getType(harmonyAssembly, "HarmonyLib.Harmony");
			harmonyMethodType = Types.getType(harmonyAssembly, "HarmonyLib.HarmonyMethod");
			//As the client still runs on net4, use this getMethod signature:
			harmonyPatchMethod = harmonyType.GetMethod("Patch", Bindings.publicInst, null, new Type[]
			{
				typeof(MethodBase),
				harmonyMethodType,
				harmonyMethodType,
				harmonyMethodType,
				harmonyMethodType,
			}, null);
		}
		
		public static object getHarmonyInstance(string name)
		{
			init();
			return Types.createInstance(harmonyType, name);
		}

		public static void patch(object harmonyInstance, MethodInfo toPatchMethod, MethodInfo prefix = null, MethodInfo postfix = null)
		{
			object prefixMethod = null;
			object postfixMethod = null;
			if(prefix != null)
			{
				prefixMethod = Types.createInstance(harmonyMethodType, prefix);
			}
			if(postfix != null)
			{
				postfixMethod = Types.createInstance(harmonyMethodType, postfix);
			}
			harmonyPatchMethod.Invoke(harmonyInstance, new object[]
			{
				toPatchMethod, prefixMethod, postfixMethod, null, null,
			});
		}

		public static void unpatchAll(object harmonyInstance)
		{
			object id = harmonyType.GetProperty("Id").GetValue(harmonyInstance);
			harmonyType.GetMethod("UnpatchAll", Bindings.publicInst, null, new Type[]
			{
				typeof(string),
			}, null).Invoke(harmonyInstance, new object[]{id});
		}
	}
}
