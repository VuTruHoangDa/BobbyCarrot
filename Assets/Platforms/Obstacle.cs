using BobbyCarrot.Movers;
using Cysharp.Threading.Tasks;
using RotaryHeart.Lib.SerializableDictionary;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Pool;
using UnityEngine.Tilemaps;


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
					animationData = windAnim;
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
			if (mover is LotusLeaf or Cloud || type == Type.Border) return false;
			if (mover is Flyer or Fireball) return true;

			return type switch
			{
				Type.Grass => mover is Truck,
				Type.Lock => mover is Bobby && PlayGround.items[Item.Type.Key] != 0,
				Type.Rock => mover is Truck && mover.speed == (mover as Truck).highSpeed,
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
					if (mover is Truck) Pop(index);
					break;

				case Type.Lock:
					if (mover is Bobby) Pop(index);
					break;

				case Type.Rock:
					if (mover is Truck) Pop(index);
					break;

				case Type.Snow:
					if (mover is Bobby) Pop(index);
					break;

				case Type.Wind:
					if (mover is not Bobby) break;

					mover.gameObject.SetActive(false);
					Mover.Show<Flyer>(mover.transform.position, mover.direction);
					break;

				default: throw new Exception();
			}
		}
	}
}