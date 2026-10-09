using BobbyCarrot.Movers;
using System;
using UnityEngine;


namespace BobbyCarrot.Platforms
{
	[CreateAssetMenu(fileName = "Obstacle", menuName = "Platforms/Obstacle")]
	public sealed class Obstacle : Platform
	{
		public enum Type
		{
			Normal, Border, Lock, Wind, Snow, Rock, Grass
		}
		public Type type { get; private set; }

		[SerializeField] private AnimationData windAnim;
		protected override Platform Create()
		{
			var p = base.Create() as Obstacle;

			switch (id)
			{
				case 84:
					p.type = Type.Wind;
					p.animationData = windAnim;
					break;

				case 109:
					p.type = Type.Rock;
					break;

				case 135:
					p.type = Type.Grass;
					break;

				case 141:
					p.type = Type.Lock;
					break;

				case 269:
					p.type = Type.Snow;
					break;

				case 374:
					p.type = Type.Border;
#if !UNITY_EDITOR
					p.sprite=null;
#endif
					break;

				default:
					p.type = Type.Normal;
					break;
			}

			return p;
		}


		public override bool CanEnter(Mover mover)
		{
			if (mover is IPlatform || type == Type.Border) return false;
			if (mover is Flyer or Fireball) return true;

			return type switch
			{
				Type.Grass => mover is Truck,
				Type.Lock => mover is Bobby && PlayGround.items[Item.Type.Key] != 0,
				Type.Rock => mover is Truck && mover.speed == Conveyor.speed,
				Type.Snow => mover is Bobby && PlayGround.items[Item.Type.Shovel] != 0,
				Type.Wind => mover is Bobby && PlayGround.items[Item.Type.Kite] != 0,
				_ => false,
			};
		}


		public override void OnEnter(Mover mover)
		{
			if (mover is Flyer or Fireball) return;

			switch (type)
			{
				case Type.Grass:
					if (mover is Truck) Pop(position);
					break;

				case Type.Lock:
					if (mover is Bobby) Pop(position);
					break;

				case Type.Rock:
					if (mover is Truck) Pop(position);
					break;

				case Type.Snow:
					if (mover is Bobby) Pop(position);
					break;

				case Type.Wind:
					if (mover is not Bobby) break;

					var dir = mover.direction;
					mover.gameObject.SetActive(false);
					Mover.Show<Flyer>(position, dir);
					break;

				default: throw new Exception();
			}
		}
	}
}