using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;


namespace BobbyCarrot.Editor
{
	public static class Tool
	{
		[MenuItem("Tools/Print Sorting Layer IDs")]
		private static void PrintSortingLayerIDs()
		{
			var internalEditorUtility = typeof(InternalEditorUtility);
			var layers = internalEditorUtility.GetProperty("sortingLayerNames", BindingFlags.Static | BindingFlags.NonPublic)
				.GetValue(null, new object[0]) as string[];
			var ids = internalEditorUtility.GetProperty("sortingLayerUniqueIDs", BindingFlags.Static | BindingFlags.NonPublic)
				.GetValue(null, new object[0]) as int[];

			for (int i = 0; i < layers.Length; ++i) Debug.Log($"{layers[i]}: {ids[i]}");
		}
	}
}
