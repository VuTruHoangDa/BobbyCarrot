using BobbyCarrot.Movers;
using Cysharp.Threading.Tasks;
using RotaryHeart.Lib.SerializableDictionary;
using System.Collections.Generic;
using System.Threading;
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


		private static WayPoint wayPoint;
		public const float speed = 0.1f;
		public override async void OnEnter(Mover mover)
		{
			if (wayPoint != null || mover is Flyer or Fireball) return;

			FindWayPoint(position);
			if (wayPoint == null) return;

			Main.Unregister(mover as IPlayer);
			Push(wayPoint);
			mover.speed = speed;
			var tokenPlayGround = PlayGround.Token;
			var tokenMover = mover.Token;
			(mover as IPlayer).dpad = direction;
			while (!tokenPlayGround.IsCancellationRequested && !tokenMover.IsCancellationRequested
				&& mover.direction != default && wayPoint != null) await UniTask.Yield();

			if (tokenPlayGround.IsCancellationRequested || wayPoint == null) return;

			Pop(wayPoint.position);
			wayPoint = null;
			if (tokenMover.IsCancellationRequested) return;

			mover.speed = (mover as IPlayer).originalSpeed;
			Main.Register(mover as IPlayer);


			void FindWayPoint(Vector3 pos)
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
				{
					if ((pos -= direction) != position) wayPoint = new() { position = pos };
					return;
				}

				// cản 1 == Rock(Truck) hoặc platform khác. Tiến 1 bước, kiểm tra platform cản 2
				// cản 2: Border, Rock, băng chuyền cùng chiều hoặc khác chiều, platform khác
				platform = Peek(pos += direction);
				o = platform as Obstacle;
				c = platform as Conveyor;
				if ((o && o.type == Obstacle.Type.Border)
					|| (c && c.direction != direction)
					|| (o && o.type == Obstacle.Type.Rock && mover is Bobby))
				{
					wayPoint = new() { position = pos - direction };
					return;
				}

				if (c && c.direction == direction) FindWayPoint(pos);
				else wayPoint = new() { position = pos };
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
			wayPoint = null;
		};



		private sealed class WayPoint : IWayPoint
		{
			public Vector3 position { get; set; }


			public void OnEnter(Mover mover)
			{
				Pop(position);
				wayPoint = null;
				var platform = Peek(position);
				if (platform is Conveyor)
				{
					mover.speed = (mover as IPlayer).originalSpeed;
					Main.Register(mover as IPlayer);
					return;
				}

				using var cts = CancellationTokenSource.CreateLinkedTokenSource(mover.Token, PlayGround.Token);
				platform.OnEnter(mover);
				if (cts.IsCancellationRequested) return;

				mover.speed = (mover as IPlayer).originalSpeed;
				Main.Register(mover as IPlayer);
			}


			public bool CanExit(Mover mover) => true;


			public void OnExit(Mover mover) { }


			public bool CanEnter(Mover mover) => true;
		}
	}
}