using BobbyCarrot.Movers;
using RotaryHeart.Lib.SerializableDictionary;
using System.Collections.Generic;
using UnityEngine;


namespace BobbyCarrot.Platforms
{
	[CreateAssetMenu(fileName = "Block", menuName = "Platforms/Block")]
	public sealed class Block : Platform
	{
		[SerializeField] private SerializableDictionaryBase<Color, SerializableDictionaryBase<bool, Sprite>> sprites;
		private Color color;
		private bool on;

		protected override Platform Create()
		{
			var p = base.Create() as Block;
			p.sprites = sprites;

			switch (id)
			{
				case 131:
					p.color = Color.Yellow;
					p.on = true;
					break;

				case 132:
					p.color = Color.Yellow;
					p.on = false;
					break;

				case 133:
					p.color = Color.Red;
					p.on = true;
					break;

				default:
					p.color = Color.Red;
					p.on = false;
					break;
			}
			p.sprite = sprites[p.color][p.on];
			blocks[p.color].Add(p);

			return p;
		}


		public override bool CanEnter(Mover mover) =>
			mover is not LotusLeaf and not Cloud && (mover is Flyer or Fireball || !on);


		private static readonly IReadOnlyDictionary<Color, List<Block>> blocks = new Dictionary<Color, List<Block>>
		{
			[Color.Yellow] = new(),
			[Color.Red] = new()
		};
		public static void ChangeState(Color color)
		{
			foreach (var block in blocks[color])
				block.sprite = block.sprites[color][block.on = !block.on];
		}


		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void Init()
		{
			PlayGround.onAwake += () =>
			{
				blocks[Color.Yellow].Clear();
				blocks[Color.Red].Clear();
			};
		}
	}
}