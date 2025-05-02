using BobbyCarrot.Movers;
using Cysharp.Threading.Tasks;
using RotaryHeart.Lib.SerializableDictionary;
using System.Collections.Generic;
using UnityEngine;


namespace BobbyCarrot.Platforms
{
	[CreateAssetMenu(fileName = "WaterFlow", menuName = "Platforms/WaterFlow")]
	public sealed class WaterFlow : Platform
	{
		private static readonly List<WaterFlow> flows = new();

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void Init()
		{
			PlayGround.onAwake += () => flows.Clear();
		}


		[SerializeField] private SerializableDictionaryBase<Vector3, AnimationData> anims;

		public Vector3 direction { get; private set; }

		protected override Platform Create()
		{
			var p = base.Create() as WaterFlow;
			p.anims = anims;

			p.direction = id switch
			{
				247 => Vector3.down,
				248 => Vector3.up,
				249 => Vector3.right,
				_ => Vector3.left
			};
			p.animationData = anims[p.direction];
			flows.Add(p);
			if (flows.Count == 1) CheckAllFlows(true);

			return p;
		}


		public static readonly new int TASK_ID = "WaterFlow.CheckAllFlows".GetHashCode();

		private static async void CheckAllFlows(bool gameBusy = false)
		{
			if (gameBusy)
			{
				var token = PlayGround.Token;
				PlayGround.taskList.Add(TASK_ID);
				while (!token.IsCancellationRequested
					&& PlayGround.taskList.Contains(Platform.TASK_ID)) await UniTask.Yield();
				if (token.IsCancellationRequested) return;
				PlayGround.taskList.Remove(TASK_ID);
			}

			foreach (var flow in flows.Random())
			{
				var lotusLeaf = Peek(flow.index) as LotusLeaf;
				if (lotusLeaf && lotusLeaf.direction == default) lotusLeaf.Move(flow.direction);
			}
		}


		public override bool CanEnter(Mover mover) =>
			mover is Flyer or Fireball || (mover is LotusLeaf && -direction != mover.direction);


		public override bool CanExit(Mover mover) =>
			mover is not LotusLeaf || (-mover.direction != direction);


		public override void OnEnter(Mover mover)
		{
			if (mover is LotusLeaf) (mover as LotusLeaf).Move(direction);
		}


		public static void ChangeState()
		{
			foreach (var flow in flows)
			{
				flow.direction = -flow.direction;
				flow.animationData = flow.anims[flow.direction];
			}

			CheckAllFlows();
		}
	}
}