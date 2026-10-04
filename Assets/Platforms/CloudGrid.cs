using BobbyCarrot.Movers;
using RotaryHeart.Lib.SerializableDictionary;
using UnityEngine;


namespace BobbyCarrot.Platforms
{
	[CreateAssetMenu(fileName = "CloudGrid", menuName = "Platforms/CloudGrid")]
	public sealed class CloudGrid : Platform
	{
		public Color color { get; private set; }
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


		public override bool CanEnter(Mover mover) => mover is Flyer or Fireball or Cloud;


		public override void OnEnter(Mover mover)
		{
			if (mover is Cloud cloud && cloud.color == color) cloud.Lock();
		}
	}
}