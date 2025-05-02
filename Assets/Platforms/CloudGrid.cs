using BobbyCarrot.Movers;
using Cysharp.Threading.Tasks;
using RotaryHeart.Lib.SerializableDictionary;
using UnityEngine;


namespace BobbyCarrot.Platforms
{
	[CreateAssetMenu(fileName = "CloudGrid", menuName = "Platforms/CloudGrid")]
	public sealed class CloudGrid : Platform
	{
		private Color color;
		[SerializeField] private SerializableDictionaryBase<Color, Sprite> sprites;
		protected override Platform Create()
		{
			var p = base.Create() as CloudGrid;
			p.sprites = sprites;

			p.color = id switch
			{
				80 => Color.Red,
				81 => Color.Violet,
				_ => Color.Green
			};
			p.sprite = sprites[p.color];

			return p;
		}


		public override bool CanEnter(Mover mover) =>
			mover is Flyer or Fireball or Cloud;


		public override void OnEnter(Mover mover)
		{
			if (mover is Flyer || mover is Fireball || (mover as Cloud).color != color) return;

			var cloud = Peek(index) as Cloud;
			cloud.Move(default);
			cloud.speed = 0;
		}
	}
}