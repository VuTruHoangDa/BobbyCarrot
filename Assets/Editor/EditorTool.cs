using UnityEditor;
using UnityEngine;


namespace BobbyCarrot.Editors
{
	public static class SpriteSlicer
	{
		private const int SIZE_X = 48, SIZE_Y = 48, NUM_X = 16, NUM_Y = 21;


		[MenuItem("Tools/Rename Selected Spritesheet")]
		private static void RenameSpriteSheet()
		{
			int count = 0;
			var rects = new SpriteRect[NUM_X * NUM_Y];

			for (int y = 0; y < NUM_Y; ++y)
			{
				for (int x = 0; x < NUM_X; ++x)
				{
					rects[count] = new()
					{
						name = count.ToString(),
						rect = new(x * SIZE_X, y * SIZE_Y, SIZE_X, SIZE_Y),
						alignment = SpriteAlignment.Center
					};

					++count;
				}
			}

			var factory = new UnityEditor.U2D.Sprites.SpriteDataProviderFactories();
			factory.Init();
			var provider = factory.GetSpriteEditorDataProviderFromObject(Selection.activeObject);
			provider.InitSpriteEditorDataProvider();
			provider.SetSpriteRects(rects);
			provider.Apply();
			Debug.Log("OK !");
		}
	}
}