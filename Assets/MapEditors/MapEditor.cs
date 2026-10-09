using System;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Tilemaps;


namespace BobbyCarrot.MapEditors
{
	public sealed class MapEditor : MonoBehaviour
	{
		[SerializeField] private Tilemap top, mid, bottom;


		public string CreateMapFile()
		{
			// Kiểm tra data hợp lệ:
			// Tất cả FanButton nếu cùng màu thì phải cùng trạng thái Bật/Tắt
			// Chỉ có duy nhất 1 Start Point, 1 Exit Point
			// Wood (nếu có) phải ở trên cùng
			// Quạt vàng và quạt đỏ không thể đối diện nhau trên đường thẳng => mây sẽ không biết di chuyển thế nào ?
			// Đặt Gương (Mirror) sao cho cầu lửa không tồn tại mãi, cuối cùng sẽ biến mất
			// Người chơi có thể thay đổi Mirror để cho cầu lửa biến mất
			// Đầu Rồng lửa không sát vật cản, cầu lửa luôn có thể đi tối thiểu 1 bước
			// Gió (Wind) không thể sát vật cản => Flyer luôn có thể đi tối thiểu 1 bước

			var sb = new StringBuilder();
			using var writer = new StringWriter(sb);

			bottom.CompressBounds();
			mid.CompressBounds();
			top.CompressBounds();
			var min = bottom.cellBounds.min;
			var max = bottom.cellBounds.max;
			var pos = new Vector3Int();
			for (pos.x = min.x; pos.x < max.x; ++pos.x)
			{
				for (pos.y = min.y; pos.y < max.y; ++pos.y)
				{
					var tile = bottom.GetTile(pos);
					if (!tile) throw new Exception("Bottom phải luôn có Tile !");
					writer.Write(tile.name);

					tile = mid.GetTile(pos);
					if (tile) writer.Write($",{tile.name}");

					var topTile = top.GetTile(pos);
					if (topTile)
					{
						if (!tile) throw new Exception("Nếu Top có tile thì Mid phải có tile !");
						writer.Write($",{topTile.name}");
					}

					writer.Write(' ');
				}

				sb.Remove(sb.Length - 1, 1);
				writer.WriteLine();
			}

			return sb.ToString();
		}
	}
}