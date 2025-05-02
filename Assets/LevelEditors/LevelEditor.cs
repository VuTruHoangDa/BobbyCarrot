using System;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Tilemaps;


namespace BobbyCarrot.LevelEditors
{
	public sealed class LevelEditor : MonoBehaviour
	{
		[SerializeField] private Tilemap top, mid, bottom;


		public string CreateLevelFile()
		{
			var sb = new StringBuilder();
			using var writer = new StringWriter(sb);

			bottom.CompressBounds();
			mid.CompressBounds();
			top.CompressBounds();
			var min = bottom.cellBounds.min;
			var max = bottom.cellBounds.max;
			var index = new Vector3Int();
			for (index.x = min.x; index.x < max.x; ++index.x)
			{
				for (index.y = min.y; index.y < max.y; ++index.y)
				{
					var tile = bottom.GetTile(index);
					if (!tile) throw new Exception("Bottom phải luôn có Tile !");
					writer.Write(Convert.ToUInt16(tile.name));

					tile = mid.GetTile(index);
					if (tile) writer.Write($",{Convert.ToUInt16(tile.name)}");

					var topTile = top.GetTile(index);
					if (topTile)
					{
						if (!tile) throw new Exception("Nếu Top có tile thì Mid phải có tile !");
						writer.Write($",{Convert.ToUInt16(topTile.name)}");
					}

					writer.Write(" ");
				}

				sb.Remove(sb.Length - 1, 1);
				writer.WriteLine();
			}

			return sb.ToString();
		}
	}
}