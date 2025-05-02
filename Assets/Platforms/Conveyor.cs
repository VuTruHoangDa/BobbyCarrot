using BobbyCarrot.Movers;
using RotaryHeart.Lib.SerializableDictionary;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;


namespace BobbyCarrot.Platforms
{
	[CreateAssetMenu(fileName = "Conveyor", menuName = "Platforms/Conveyor")]
	public sealed class Conveyor : Platform
	{
		private static readonly List<Conveyor> conveyors = new();
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void Init() => PlayGround.onAwake += () => conveyors.Clear();


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


		public override bool CanEnter(Mover mover) =>
			mover is not LotusLeaf and not Cloud
			&& (mover is Flyer or Fireball || mover.direction == direction);


		private static StopPoint stopPoint;
		private static CancellationTokenSource cts;
		public override void OnEnter(Mover mover)
		{
			if (stopPoint != null || mover is Flyer or Fireball) return;

			// Quét theo hướng Conveyor > tìm điểm kết thúc di chuyển (ngoài Conveyor)
			Vector3 i = index;
			while (Peek(i += direction) is Conveyor c && c.direction == direction) ;

			// Cài Stop Point. Mover trượt quán tính thêm 1 bước nữa nếu có thể.
			stopPoint = new()
			{
				originalSpeed = mover.speed,
				index = !Peek(i).CanEnter(mover) ? i - direction
				: Peek(i + direction).CanEnter(mover) ? i + direction
				: i
			};
			Push(stopPoint.index, stopPoint);

			// Hủy Gamepad dpad, di chuyển mover tốc độ nhanh hơn
			Main.RemoveListener(mover as IGamepadListener);
			//mover.speed=

			// Nếu mover/PlayGround bị hủy thì khôi phục mover, xóa hết
			(cts = CancellationTokenSource.CreateLinkedTokenSource(mover.Token, PlayGround.Token))
				.Token.Register(() => Cleanup(mover));
			(mover as IGamepadListener).dpad = direction;
		}


		public static void ChangeState()
		{
			foreach (var conveyor in conveyors)
				conveyor.animationData = conveyor.anims[conveyor.direction = -conveyor.direction];
		}


		private static void Cleanup(Mover mover)
		{
			mover.speed = stopPoint.originalSpeed;
			(mover as IGamepadListener).dpad = default;
			if (!PlayGround.Token.IsCancellationRequested && mover.gameObject.activeSelf)
				Main.AddListener(mover as IGamepadListener);

			Pop(stopPoint.index);
			stopPoint = null;
			cts.Dispose();
			cts = null;
		}



		private sealed class StopPoint : IPlatform
		{
			public bool CanEnter(Mover mover) => true;


			public Vector3 index;
			public float originalSpeed;
			public void OnEnter(Mover mover)
			{
				Cleanup(mover);
				Peek(index).OnEnter(mover);
			}


			public bool CanExit(Mover mover) => true;
			public void OnExit(Mover mover) { }
		}
	}
}