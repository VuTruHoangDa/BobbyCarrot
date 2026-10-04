using BobbyCarrot.Platforms;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;


namespace BobbyCarrot
{
	public static class Util
	{
		public const int Layer_Mover = 335754105, Layer_UI = 1800765365;


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3Int ToVector3Int(this in Vector3 value) => new((int)value.x, (int)value.y, 0);


		public static T[][] NewArray<T>(int sizeX, int sizeY, Func<int, int, T> initialize = null)
		{
			var array = new T[sizeX][];
			for (int x = 0; x < sizeX; ++x)
			{
				array[x] = new T[sizeY];
				if (initialize != null)
					for (int y = 0; y < sizeY; ++y) array[x][y] = initialize(x, y);
			}

			return array;
		}


		public static bool isRunning(this UniTask task)
		{
			try
			{
				if (task.Status == UniTaskStatus.Faulted) goto FAULTED;
				return task.Status == UniTaskStatus.Pending;
			}
			catch (InvalidOperationException) { return false; }

		FAULTED:
			task.Forget();
			return false;
		}


		public static bool isRunning<T>(this UniTask<T> task)
		{
			try
			{
				if (task.Status == UniTaskStatus.Faulted) goto FAULTED;
				return task.Status == UniTaskStatus.Pending;
			}
			catch (InvalidOperationException) { return false; }

		FAULTED:
			task.Forget();
			return false;
		}


		public static IEnumerable<T> Random<T>(this IReadOnlyCollection<T> collection)
		{
			if (collection.Count == 0) yield break;

			var tmp = new List<T>(collection);
			do
			{
				int index = UnityEngine.Random.Range(0, tmp.Count);
				yield return tmp[index];
				tmp.RemoveAt(index);
			} while (tmp.Count != 0);
		}
	}



	public enum Color
	{
		Yellow, Red, Green, Violet
	}



	public interface IWayPoint : IPlatform
	{
		Vector3 position { get; }
	}
}