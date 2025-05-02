using BobbyCarrot.Movers;
using Cysharp.Threading.Tasks;
using NUnit.Framework.Constraints;
using RotaryHeart.Lib.SerializableDictionary;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;


namespace BobbyCarrot.Platforms
{
	[CreateAssetMenu(fileName = "PinWheel", menuName = "Platforms/PinWheel")]
	public sealed class PinWheel : Platform
	{
		public static readonly IReadOnlyDictionary<Color, IReadOnlyList<PinWheel>> dict = new Dictionary<Color, IReadOnlyList<PinWheel>>
		{
			[Color.Yellow] = new List<PinWheel>(),
			[Color.Red] = new List<PinWheel>(),
			[Color.Green] = new List<PinWheel>(),
			[Color.Violet] = new List<PinWheel>()
		};

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void Init()
		{
			PlayGround.onAwake += () =>
				{
					foreach (Color color in Enum.GetValues(typeof(Color)))
						(dict[color] as List<PinWheel>).Clear();
				};
		}


		public Color color { get; private set; }

		[SerializeField] private SerializableDictionaryBase<Color, AnimationData> anims;


		[SerializeField] private SerializableDictionaryBase<Color, Sprite> sprites;
		protected override Platform Create()
		{
			var p = base.Create() as PinWheel;
			p.sprites = sprites;
			p.anims = anims;

			p.color = id == 112 ? Color.Yellow
				: id == 113 ? Color.Red
				: id == 114 ? Color.Green : Color.Violet;
			p.sprite = sprites[p.color];
			p.UI = Addressables.InstantiateAsync("Assets/Platforms/Prefab/PinWheel UI.prefab",
				index, Quaternion.identity).WaitForCompletion();
			p.UI.transform.parent = anchor;
			p.UI.transform.GetChild((int)p.color).gameObject.SetActive(true);
			(dict[p.color] as List<PinWheel>).Add(p);

			return p;
		}


		public override bool CanEnter(Mover mover) => mover is Flyer or Fireball;


		public static readonly IReadOnlyDictionary<Color, Vector3> DIRECTIONS = new Dictionary<Color, Vector3>
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
			Vector3 pos = index;
			var dir = DIRECTIONS[color];
			IPlatform platform;

			do
			{
				pos += dir;
				platform = Peek(pos);
				if (platform is not Cloud) continue;

				var cloud = platform as Cloud;
				if (cloud.speed > 0 && cloud.direction == default &&
					(color == Color.Yellow || cloud.color == color)) cloud.Move(dir);
			} while (platform is not Obstacle || (platform as Obstacle).type != Obstacle.Type.Border);
		}
	}
}