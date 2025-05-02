using System;
using System.Collections.Generic;
using System.IO;


namespace BobbyCarrot
{
	public readonly struct Level
	{
		public readonly ReadOnlyArray<ReadOnlyArray<ReadOnlyArray<ushort>>> platforms;


		private static readonly Stack<ushort[]> stack = new();
		public Level(string data)
		{
			// Kiểm tra data hợp lệ:
			// Tất cả PinWheelButton nếu cùng màu thì phải cùng trạng thái Bật/Tắt
			// Cloud, LotusLeaf không thể bị đè lên bởi platform nào khác


			using var reader = new StringReader(data);
			string[] words;
			var line = reader.ReadLine();
			int NUM_Y = line.Split(' ').Length;
			int NUM_X = 0;

			// data -> stack
			do
			{
				++NUM_X;
				words = line.Split(' ');
				for (int y = 0; y < NUM_Y; ++y)
				{
					var w = words[y].Split(',');
					var a = new ushort[w.Length];
					int i = 0;
					foreach (var number in w) a[i++] = Convert.ToUInt16(number);
					stack.Push(a);
				}
			} while ((line = reader.ReadLine()) != null);

			// stack -> array
			var array = new ReadOnlyArray<ReadOnlyArray<ushort>>[NUM_X];
			for (int x = NUM_X - 1; x >= 0; --x)
			{
				var a = new ReadOnlyArray<ushort>[NUM_Y];
				array[x] = new(a);
				for (int y = NUM_Y - 1; y >= 0; --y) a[y] = new(stack.Pop());
			}

			platforms = new(array);
		}


		public int width => platforms.Length;

		public int height => platforms[0].Length;
	}
}