using BobbyCarrot.Movers;
using RotaryHeart.Lib.SerializableDictionary;
using System.Collections.Generic;
using UnityEngine;


namespace BobbyCarrot.Platforms
{
	[CreateAssetMenu(fileName = "Fan", menuName = "Platforms/Fan")]
	public sealed class Fan : Platform
	{
		public static readonly IReadOnlyDictionary<Color, IReadOnlyList<Fan>> fans = new Dictionary<Color, IReadOnlyList<Fan>>
		{
			[Color.Yellow] = new List<Fan>(),
			[Color.Red] = new List<Fan>(),
			[Color.Green] = new List<Fan>(),
			[Color.Violet] = new List<Fan>()
		};

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void Init() =>
			PlayGround.onAwake += () =>
				{
					foreach (var color in fans.Keys) (fans[color] as List<Fan>).Clear();
				};


		public Color color { get; private set; }

		/// <summary>
		/// Chú ý: quạt tắt thì direction == 0
		/// </summary>
		public Vector3 direction => on ? directions[color] : default;

		[SerializeField] private SerializableDictionaryBase<Color, AnimationData> anims;
		[SerializeField] private SerializableDictionaryBase<Color, Sprite> sprites;
		protected override Platform Create()
		{
			var p = base.Create() as Fan;
			p.sprites = sprites;
			p.anims = anims;

			p.color = id switch
			{
				112 => Color.Yellow,
				113 => Color.Red,
				114 => Color.Green,
				_ => Color.Violet
			};
			p.UI = Instantiate(UI, position + directions[p.color], p.color switch
			{
				Color.Green => Quaternion.Euler(0, 0, 90),
				Color.Red => Quaternion.Euler(0, 0, 180),
				Color.Violet => Quaternion.Euler(0, 0, -90),
				_ => Quaternion.identity
			});
			p.UI.transform.parent = anchor;
			p.UI.SetActive(false);
			(fans[p.color] as List<Fan>).Add(p);

			return p;
		}


		public override bool CanEnter(Mover mover) => mover is Flyer or Fireball;


		public static readonly IReadOnlyDictionary<Color, Vector3> directions = new Dictionary<Color, Vector3>
		{
			[Color.Yellow] = Vector3.up,
			[Color.Red] = Vector3.down,
			[Color.Green] = Vector3.left,
			[Color.Violet] = Vector3.right
		};
		public bool on { get; private set; }


		[SerializeField] private GameObject UI;
		public void ChangeState(bool on)
		{
			UI.SetActive(this.on = on);
			animationData = on ? anims[color] : default;
			if (!on)
			{
				sprite = sprites[color];
				return;
			}

			// ON STATE
			var pos = position;
			var dir = direction;
			IPlatform platform;

			do
			{
				platform = Peek(pos += dir);
				if (platform is Cloud cloud && !cloud.isLock && cloud.direction == default
					&& (color == Color.Yellow || cloud.color == color)) cloud.Move(this);
			} while (platform is not Obstacle || (platform as Obstacle).type != Obstacle.Type.Border);
		}
	}
}