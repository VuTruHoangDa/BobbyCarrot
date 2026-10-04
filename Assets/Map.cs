using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine.InputSystem.Utilities;


namespace BobbyCarrot
{
	public readonly struct Map
	{
		public readonly ReadOnlyArray<ReadOnlyArray<ReadOnlyArray<ushort>>> platforms;


		private static readonly Stack<ushort[]> stack = new();
		public Map(string data)
		{
			// Map Editor trong game hoặc Map Editor standalone (ngoài game) sẽ đảm bảo data hợp lệ

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


		public int width => platforms.Count;

		public int height => platforms[0].Count;
	}
}