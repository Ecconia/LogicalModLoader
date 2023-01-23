using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LogicalModLoader.Client.GameObjectDebugging
{
	public static class YetAnotherGODebugger
	{
		public static void debug()
		{
			//Print everything:
			HashSet<GameObject> seenObj = new HashSet<GameObject>();
			StringBuilder sb = new StringBuilder();
			for(int i = 0; i < SceneManager.sceneCount; i++)
			{
				Scene scene = SceneManager.GetSceneAt(i);
				sb.Append("- " + i + ": " + scene.name + " (" + (scene.isLoaded ? "loaded" : "unloaded") + ")\n");
				foreach(var child in scene.GetRootGameObjects())
				{
					recurse(child, "\t");
				}
			}
			sb.Append("=> Remaining objects:\n");
			foreach(var go in Object.FindObjectsOfType<GameObject>())
			{
				var go2 = getParent(go);
				if(seenObj.Contains(go2))
				{
					continue;
				}
				recurse(go, "\t");
			}
			Debug.Log(sb.ToString());

			GameObject getParent(GameObject go)
			{
				return go.transform.parent == null ? go : getParent(go.transform.parent.gameObject);
			}
			void recurse(GameObject go, string prefix)
			{
				seenObj.Add(go);
				sb.Append(prefix).Append("- " + go.name).Append('\n');
				prefix += "\t";
				foreach(Transform child in go.transform)
				{
					recurse(child.gameObject, prefix);
				}
			}
		}
	}
}
