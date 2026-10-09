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
			Land, Water, Sky, Ice, Exit, WindStop, DragonTail
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
			else if (id == 181) startPoint = position;

			p.dragonAnim = dragonAnim;

			return p;
		}


		public override bool CanEnter(Mover mover) =>
			mover is Flyer or Fireball || ((mover is LotusLeaf) ? (type == Type.Water)
			: (mover is Cloud) ? (type == Type.Sky) : (type != Type.Sky && type != Type.Water));


		[SerializeField] private AnimationData dragonAnim;
		private const float speed = 0.099f;
		private static bool slipping;
		public override async void OnEnter(Mover mover)
		{
			switch (type)
			{
				case Type.DragonTail:
					{
						if (mover is not Bobby and not Truck) return;

						#region Bắn cầu lửa, chỉ 1 cầu lửa được bật
						var fireBall = Mover.Get<Fireball>();
						if (fireBall && fireBall.gameObject.activeSelf) return;

						var head = Peek(new(position.x - 2, position.y)) as Platform;
						head.animationData = dragonAnim;
						fireBall = Mover.Show<Fireball>(head.position, Vector3.left);

						// Nếu mover di chuyển thì focus camera theo mover
						var tokenPlayGround = PlayGround.Token;
						var tokenFireBall = fireBall.Token;
						while (!tokenPlayGround.IsCancellationRequested && !tokenFireBall.IsCancellationRequested
							&& mover.transform.position == position) await UniTask.Yield();

						if (!tokenPlayGround.IsCancellationRequested) Camera.Focus(mover);
						#endregion
					}
					break;

				case Type.Ice:
					{
						if (slipping || mover is not Bobby) return;

						#region Bobby trượt 1 bước theo mover.direction (nếu có thể)
						// Tìm dest
						var dest = position;
						var dir = mover.direction;
						IPlatform platform;
						do platform = Peek(dest += dir);
						while (platform is Ground g && g.type == Type.Ice);

						// Vật cản là Border hoặc khác
						if (platform is Obstacle o && o.type == Obstacle.Type.Border)
							if ((dest -= dir) == position) return;

						slipping = true;
						Main.Unregister(mover as IPlayer);
						if (mover.speed < speed) mover.speed = speed;
						var tokenMover = mover.Token;
						tokenMover.Register(() => slipping = false);
						(mover as IPlayer).Move(dir, dest);
						var magnitude = (dest - position).sqrMagnitude;
						var tokenPlayGround = PlayGround.Token;
						while (!tokenPlayGround.IsCancellationRequested && !tokenMover.IsCancellationRequested
							&& mover.direction != default && (mover.transform.position - position).sqrMagnitude < magnitude)
							await UniTask.Yield();

						if (tokenPlayGround.IsCancellationRequested || tokenMover.IsCancellationRequested) return;

						slipping = false;
						if (mover.direction == default)
						{
							mover.speed = (mover as IPlayer).originalSpeed;
							Main.Register(mover as IPlayer);
						}
						#endregion
					}
					break;

				case Type.Exit:
					if (mover is not Bobby) break;

					// Nếu đủ Carrot hoặc Egg == 0 thì kết thúc màn chơi (PlayGround.End)
					break;

				case Type.WindStop:
					{
						if (mover is not Flyer) break;

						var dir = mover.direction;
						mover.gameObject.SetActive(false);
						Mover.Show<Bobby>(position, dir);
					}
					break;
			}
		}


		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void Init() => PlayGround.onAwake += () => slipping = false;
	}
}