using BobbyCarrot.Platforms;
using Cysharp.Threading.Tasks;
using RotaryHeart.Lib.SerializableDictionary;
using System;
using UnityEngine;


namespace BobbyCarrot.Movers
{
	public sealed class Cloud : Mover, IPlatform
	{
		public async void Move(Vector3 direction)
		{
			if (this.direction != default) throw new System.Exception("Mây đang trong bước di chuyển, không thể thay đổi moverDirection !");
			direction.CheckValidDpad();
			if ((this.direction = direction) != default) await Move();
		}


		protected override async UniTask<bool> Move()
		{
			while (CanMove())
			{
				if (!await base.Move()) return false;

				// Quét tìm PinWheel > Cập nhật moverDirection
				var pos = transform.position;
				Vector3 newDir = default;
				if (direction != PinWheel.DIRECTIONS[Color.Yellow])
					foreach (var pinWheel in PinWheel.dict[Color.Yellow])
						if (pinWheel.on && Find(Color.Yellow, pos, pinWheel.index))
						{
							newDir = PinWheel.DIRECTIONS[Color.Yellow];
							break;
						}

				if (direction != PinWheel.DIRECTIONS[color])
					foreach (var pinWheel in PinWheel.dict[color])
						if (pinWheel.on && Find(color, pos, pinWheel.index))
						{
							newDir = PinWheel.DIRECTIONS[color];
							break;
						}

				if (newDir == default) continue;
				if (-newDir == direction)
				{
					// Hướng di chuyển mới đối ngược với hướng hiện tại
					// Mây đứng yên hoặc di chuyển tùy theo các chong chóng đối nghịch
					throw new NotImplementedException();
				}
				else direction = newDir;
			}

			direction = default;
			return true;

			static bool Find(Color color, in Vector3 pos, in Vector3 wheelPos) => color switch
			{
				Color.Green => pos.x < wheelPos.x && pos.y == wheelPos.y,
				Color.Red => pos.x == wheelPos.x && pos.y < wheelPos.y,
				Color.Violet => pos.x > wheelPos.x && pos.y == wheelPos.y,
				_ => pos.x == wheelPos.x && pos.y > wheelPos.y,
			};
		}


		public Color color { get; private set; }
		[SerializeField] private SerializableDictionaryBase<Color, Sprite> sprites;
		private new void Awake()
		{
			base.Awake();
			color = Platform.id == 96 ? Color.Red
				: Platform.id == 97 ? Color.Violet : Color.Green;
			spriteRenderer.sprite = sprites[color];
		}


		public bool CanEnter(Mover mover) => mover is Flyer or Fireball ||
			(direction == default && mover is Bobby or Truck);


		public void OnEnter(Mover mover) { }


		public bool CanExit(Mover mover) => true;


		public void OnExit(Mover mover) { }
	}
}