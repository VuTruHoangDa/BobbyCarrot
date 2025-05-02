using BobbyCarrot.Movers;
using RotaryHeart.Lib.SerializableDictionary;
using UnityEngine;


namespace BobbyCarrot.Platforms
{
	[CreateAssetMenu(fileName = "Egg", menuName = "Platforms/Egg")]
	public sealed class Egg : Platform
	{
		[SerializeField] private SerializableDictionaryBase<bool, Sprite> sprites;
		private bool hasEgg;

		protected override Platform Create()
		{
			var p = base.Create() as Egg;
			p.sprites = sprites;
			p.hasEgg = id == 140;
			return p;
		}


		public override bool CanEnter(Mover mover) =>
			mover is not LotusLeaf and not Cloud && (mover is Flyer or Fireball || !hasEgg);


		public override void OnExit(Mover mover)
		{
			if (mover is Flyer or Fireball) return;

			sprite = sprites[hasEgg = true];
			--PlayGround.egg;
		}
	}
}