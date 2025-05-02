using BobbyCarrot.Movers;
using Cysharp.Threading.Tasks;
using RotaryHeart.Lib.SerializableDictionary;
using UnityEngine;


namespace BobbyCarrot.Platforms
{
	[CreateAssetMenu(fileName = "BlockButton", menuName = "Platforms/BlockButton")]
	public sealed class BlockButton : Platform
	{
		[SerializeField] private SerializableDictionaryBase<Color, SerializableDictionaryBase<bool, Sprite>> sprites;
		private Color color;
		private bool on;


		protected override Platform Create()
		{
			var p = base.Create() as BlockButton;
			p.sprites = sprites;

			switch (id)
			{
				case 159:
					p.color = Color.Yellow;
					p.on = true;
					break;

				case 128:
					p.color = Color.Yellow;
					p.on = false;
					break;

				case 129:
					p.color = Color.Red;
					p.on = true;
					break;

				default:
					p.color = Color.Red;
					p.on = false;
					break;
			}

			p.sprite = sprites[p.color][p.on];
			return p;
		}


		public override bool CanEnter(Mover mover) =>
			mover is not LotusLeaf and not Cloud;


		public override void OnEnter(Mover mover)
		{
			if (mover is Flyer or Fireball) return;

			sprite = sprites[color][on = !on];
			Block.ChangeState(color);
		}
	}
}