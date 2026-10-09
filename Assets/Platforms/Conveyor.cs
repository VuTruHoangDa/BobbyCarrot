using BobbyCarrot.Movers;
using Cysharp.Threading.Tasks;
using RotaryHeart.Lib.SerializableDictionary;
using System.Collections.Generic;
using UnityEngine;


namespace BobbyCarrot.Platforms
{
	[CreateAssetMenu(fileName = "Conveyor", menuName = "Platforms/Conveyor")]
	public sealed class Conveyor : Platform
	{
		[SerializeField] private SerializableDictionaryBase<Vector3, AnimationData> anims;
		private Vector3 direction;
		protected override Platform Create()
		{
			var p = base.Create() as Conveyor;
			p.anims = anims;

			p.direction = id switch
			{
				149 => Vector3.up,
				150 => Vector3.down,
				151 => Vector3.left,
				_ => Vector3.right
			};
			p.animationData = anims[p.direction];
			conveyors.Add(p);
			return p;
		}


		public override bool CanEnter(Mover mover) => mover is not IPlatform && CanExit(mover);


		public const float speed = 0.1f;
		private static bool transporting;
		public override async void OnEnter(Mover mover)
		{
			if (transporting || mover is Flyer or Fireball) return;

			var dest = FindDest(position);
			if (dest == default) return;

			transporting = true;
			Main.Unregister(mover as IPlayer);
			if (mover.speed < speed) mover.speed = speed;
			var tokenPlayGround = PlayGround.Token;
			var tokenMover = mover.Token;
			tokenMover.Register(() => transporting = false);
			(mover as IPlayer).Move(direction, dest);
			var magnitude = (dest - position).sqrMagnitude;
			while (!tokenPlayGround.IsCancellationRequested && !tokenMover.IsCancellationRequested
				&& mover.direction != default && (mover.transform.position - position).sqrMagnitude < magnitude)
				await UniTask.Yield();

			if (tokenPlayGround.IsCancellationRequested || tokenMover.IsCancellationRequested) return;

			transporting = false;
			if (mover.direction == default)
			{
				mover.speed = (mover as IPlayer).originalSpeed;
				Main.Register(mover as IPlayer);
			}


			Vector3 FindDest(Vector3 pos)
			{
				// Quét theo hướng băng chuyền tìm platform cản 1:
				// Cản 1: Border, Rock, Băng chuyền khác chiều, platform khác
				IPlatform platform;
				do platform = Peek(pos += direction);
				while (platform is Conveyor conveyor && conveyor.direction == direction);

				var o = platform as Obstacle;
				var c = platform as Conveyor;
				if ((o && o.type == Obstacle.Type.Border)
					|| (c && c.direction != direction)
					|| (o && o.type == Obstacle.Type.Rock && mover is Bobby))
					return (pos -= direction) != position ? pos : default;

				// cản 1 == Rock(Truck) hoặc platform khác. Tiến 1 bước, kiểm tra platform cản 2
				// cản 2: Border, Rock, băng chuyền cùng chiều hoặc khác chiều, platform khác
				platform = Peek(pos += direction);
				o = platform as Obstacle;
				c = platform as Conveyor;
				if ((o && o.type == Obstacle.Type.Border)
					|| (c && c.direction != direction)
					|| (o && o.type == Obstacle.Type.Rock && mover is Bobby))
					return pos - direction;

				return c && c.direction == direction ? FindDest(pos) : pos;
			}
		}


		public override bool CanExit(Mover mover) => mover is Flyer or Fireball || mover.direction == direction;


		public static void ChangeStates()
		{
			foreach (var conveyor in conveyors)
				conveyor.animationData = conveyor.anims[conveyor.direction *= -1];
		}


		private static readonly List<Conveyor> conveyors = new();
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void Init() => PlayGround.onAwake += () =>
		{
			conveyors.Clear();
			transporting = false;
		};
	}
}