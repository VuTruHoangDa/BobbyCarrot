using BobbyCarrot.Movers;
using Cysharp.Threading.Tasks;
using RotaryHeart.Lib.SerializableDictionary;
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

		[SerializeField] private SerializableDictionaryBase<Type, RuntimeAnimatorController> anims;
		protected override Platform Create()
		{
			var p = base.Create() as Obstacle;
			p.anims = anims;

			switch (id)
			{
				case 84:
					p.type = Type.Wind;
					// animation
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

			switch (type)
			{
				case Type.Grass:
					// Truck
					throw new NotImplementedException();

				case Type.Lock:
					// Bobby và có chìa khóa
					throw new NotImplementedException();

				case Type.Rock:
					// Truck và moverSpeed cao
					throw new NotImplementedException();

				case Type.Snow:
					// Bobby và có xẻng
					return mover is Bobby && PlayGround.items[Item.Type.Shovel] != 0;

				case Type.Wind:
					// Bobby và có Diều
					return true; // test

				default: return false;
			}
		}


		public override void OnEnter(Mover mover)
		{
			if (mover is Flyer or Fireball) return;

			switch (type)
			{
				case Type.Grass:
					// Truck
					throw new NotImplementedException();

				case Type.Lock:
					// Bobby và có chìa khóa
					throw new NotImplementedException();

				case Type.Rock:
					// Truck và moverSpeed cao
					throw new NotImplementedException();

				case Type.Snow:
					// Bobby và có xẻng
					if (mover is Bobby) Pop(index);
					break;

				case Type.Wind:
					mover.gameObject.SetActive(false);
					Mover.Show<Flyer>(mover.transform.position, mover.direction);
					break;

				default: throw new Exception();
			}
		}
	}
}