using BobbyCarrot.Movers;
using RotaryHeart.Lib.SerializableDictionary;
using System.Collections.Generic;
using UnityEngine;


namespace BobbyCarrot.Platforms
{
	[CreateAssetMenu(fileName = "Maze", menuName = "Platforms/Maze")]
	public sealed class Maze : Platform
	{
		private static readonly List<Maze> mazes = new();
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void Init()
		{
			PlayGround.onAwake += () => mazes.Clear();
		}


		private enum Type
		{
			RightUp, LeftUp, LeftDown, RightDown, UpDown, LeftRight
		}
		private Type type;

		[SerializeField] private SerializableDictionaryBase<Type, Sprite> sprites;

		protected override Platform Create()
		{
			var p = base.Create() as Maze;
			p.sprites = sprites;

			p.type = id switch
			{
				153 => Type.RightUp,
				154 => Type.LeftUp,
				155 => Type.LeftDown,
				156 => Type.RightDown,
				157 => Type.UpDown,
				_ => Type.LeftRight,
			};
			p.sprite = sprites[p.type];
			mazes.Add(p);
			return p;
		}


		public override bool CanEnter(Mover mover)
		{
			if (mover is LotusLeaf or Cloud or Truck) return false;
			if (mover is Flyer or Fireball) return true;

			var d = mover.direction;
			return type switch
			{
				Type.RightUp => d.x < 0 || d.y < 0,
				Type.LeftUp => d.x > 0 || d.y < 0,
				Type.LeftDown => d.x > 0 || d.y > 0,
				Type.RightDown => d.x < 0 || d.y > 0,
				Type.UpDown => d.x == 0,
				_ => d.y == 0
			};
		}


		public override bool CanExit(Mover mover)
		{
			if (mover is Flyer or Fireball) return true;

			var d = mover.direction;
			return type switch
			{
				Type.RightUp => d.x > 0 || d.y > 0,
				Type.LeftUp => d.x < 0 || d.y > 0,
				Type.LeftDown => d.x < 0 || d.y < 0,
				Type.RightDown => d.x > 0 || d.y < 0,
				Type.UpDown => d.x == 0,
				_ => d.y == 0
			};
		}


		public override void OnExit(Mover mover)
		{
			if (mover is Flyer or Fireball) return;

			type = type switch
			{
				Type.RightUp => Type.RightDown,
				Type.LeftUp => Type.RightUp,
				Type.LeftDown => Type.LeftUp,
				Type.RightDown => Type.LeftDown,
				Type.UpDown => Type.LeftRight,
				_ => Type.UpDown,
			};
			sprite = sprites[type];
		}


		public static void ChangeState()
		{
			foreach (var maze in mazes) maze.OnExit(null);
		}
	}
}