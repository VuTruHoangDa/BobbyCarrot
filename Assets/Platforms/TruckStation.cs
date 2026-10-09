using BobbyCarrot.Movers;
using RotaryHeart.Lib.SerializableDictionary;
using UnityEngine;


namespace BobbyCarrot.Platforms
{
	[CreateAssetMenu(fileName = "TruckStation", menuName = "Platforms/TruckStation")]
	public sealed class TruckStation : Platform
	{
		[SerializeField] private SerializableDictionaryBase<bool, Sprite> sprites;
		private bool hasTruck;

		protected override Platform Create()
		{
			var p = base.Create() as TruckStation;
			p.sprites = sprites;
			p.hasTruck = id == 375;

			return p;
		}


		public override bool CanEnter(Mover mover) =>
			mover is not IPlatform && (mover is Flyer or Fireball || (mover is Truck ? !hasTruck :
			!hasTruck || PlayGround.items[Item.Type.Gas] != 0));


		public override void OnEnter(Mover mover)
		{
			if (mover is Flyer or Fireball || (mover is Bobby && !hasTruck)) return;

			sprite = sprites[hasTruck = !hasTruck];
			var dir = mover.direction;
			mover.gameObject.SetActive(false);
			if (mover is Bobby) Mover.Show<Truck>(position, dir);
			else Mover.Show<Bobby>(position, dir);
		}
	}
}