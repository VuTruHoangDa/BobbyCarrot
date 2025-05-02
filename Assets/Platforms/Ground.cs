using BobbyCarrot.Movers;
using Cysharp.Threading.Tasks;
using UnityEngine;


namespace BobbyCarrot.Platforms
{
	[CreateAssetMenu(fileName = "Ground", menuName = "Platforms/Ground")]
	public sealed class Ground : Platform
	{
		public enum Type
		{
			Water, Sky, Ice, Exit, WindStop, DragonTail, Land
		}
		public Type type { get; private set; }

		public static Vector3 startPoint { get; private set; }


		protected override Platform Create()
		{
			var p = base.Create() as Ground;

			if (id == 245 || id == 246 || (251 <= id && id <= 253)) p.type = Type.Water;
			else if (263 <= id && id <= 268) p.type = Type.Sky;
			else if (id == 180) p.type = Type.Ice;
			else if (id == 182) p.type = Type.Exit;
			else if (id == 85) p.type = Type.WindStop;
			else if (id == 121) p.type = Type.DragonTail;
			else if (id == 181) startPoint = index;
			else p.type = Type.Land;

			p.dragonAnim = dragonAnim;
			p.delayShowingFireBall = delayShowingFireBall;

			return p;
		}


		public override bool CanEnter(Mover mover) =>
			mover is Flyer or Fireball || ((mover is LotusLeaf) ? (type == Type.Water)
			: (mover is Cloud) ? (type == Type.Sky) : (type != Type.Sky && type != Type.Water));


		[SerializeField] private AnimationData dragonAnim;
		[SerializeField] private int delayShowingFireBall;
		public override async void OnEnter(Mover mover)
		{
			var token = PlayGround.Token;
			switch (type)
			{
				case Type.DragonTail:
					if (mover is Bobby or Truck)
					{
						#region Bắn cầu lửa và đợi cầu lửa biến mất
						// Hủy dpad Bobby/ Truck

						var head = Peek(new(index.x - 2, index.y)) as Platform;
						head.animationData = dragonAnim;
						await UniTask.Delay(delayShowingFireBall);
						if (token.IsCancellationRequested) return;

						Mover.Show<Fireball>(head.index, Vector3.left);
						var fireball = Mover.GetSingleton<Fireball>();
						if (!fireball)
						{
							// Đăng ký dpad Bobby/ Truck
							break;
						}

						fireball.Token.Register(() =>
						{
							// Đăng ký dpad Bobby/ Truck sau khi fireball biến mất
						});
						#endregion
					}
					break;

				case Type.Ice:
					if (mover is not Bobby) break;

					// Bobby bị trượt một bước theo moverDirection
					break;

				case Type.Exit:
					if (mover is not Bobby) break;

					// Nếu đủ Carrot hoặc Egg thì kết thúc trò chơi (PlayGround.End)
					break;

				case Type.WindStop:
					if (mover is not Flyer) break;

					// Flyer biến mất, hiện Bobby
					mover.gameObject.SetActive(false);
					Mover.Show<Bobby>(mover.transform.position, mover.direction);
					break;

				default: break;
			}
		}
	}
}