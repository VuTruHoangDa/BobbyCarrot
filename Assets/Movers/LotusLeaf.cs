using BobbyCarrot.Platforms;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;


namespace BobbyCarrot.Movers
{
	public sealed class LotusLeaf : Mover, IPlatform
	{
		private Vector3 newDir;
		public void Move(Vector3 direction)
		{
			if (this.direction == default)
			{
				this.direction = direction;
				Move().Forget();
			}
			else newDir = direction;
		}


		private static UniTask task;
		protected override async UniTask<bool> Move()
		{
			if (balances.ContainsKey(this)) balances.Remove(this);
			else if (woodBalances.ContainsKey(this)) woodBalances.Remove(this);
			else if (inertias.Contains(this)) inertias.Remove(this);
			++count;
			if (!task.isRunning()) task = CheckBalancesAndInertias();
			while (CanMove())
			{
				var t = base.Move();
				check = true;
				if (!await t) return false;

				if (newDir != default)
				{
					direction = newDir;
					newDir = default;
				}

				// Hiện lá sen lên nếu khởi tạo ban đầu bị ẩn bởi platform
				if (spriteRenderer.sortingLayerID == 0)
				{
					spriteRenderer.sortingLayerID = Util.Layer_Mover;
					spriteRenderer.sortingOrder = -1;
				}
			}

			--count;
			var pos = transform.position;
			if (Platform.Peek(pos, Platform.Peek(pos) is LotusLeaf ? 2 : 3) is WaterFlow waterFlow)
			{
				direction = default;

				// Kiểm tra vật cản theo hướng waterFlow.direction là lá sen khác hoặc miếng gỗ (Wood) ?
				// Và ngay dưới lá sen cản, Wood là platform có thể đi vào ?
				// => Nếu vậy thì lá sen đang cân bằng động, thêm vào balances hoặc woodBalances

				var platform = Platform.Peek(pos += waterFlow.direction);
				if (platform is LotusLeaf && Platform.Peek(pos, 2).CanEnter(this))
				{
					balances[this] = waterFlow;
					if (!task.isRunning()) task = CheckBalancesAndInertias();
				}
				else if (platform is Wood wood && Platform.Peek(pos, 2).CanEnter(this)) woodBalances[this] = (waterFlow, wood);
				return true;
			}

			if (Platform.Peek(pos += direction) is LotusLeaf leaf)
			{
				// Kiểm tra lá sen có bị cản bởi lá sen khác đang di chuyển (cùng hướng/ vuông góc) ?
				// Và ngay dưới lá sen cản là platform có thể đi vào ?
				// => Nếu vậy thì lá sen đang có quán tính nhưng tạm thời bị cản, thêm vào inertias

				if (leaf.direction != default && leaf.direction != -direction && Platform.Peek(pos, 2).CanEnter(this))
				{
					inertias.Add(this);
					if (!task.isRunning()) task = CheckBalancesAndInertias();
					return true;
				}

				direction = default;
				return true;
			}

			direction = default;
			return true;
		}


		public bool CanEnter(Mover mover) =>
			mover is Flyer or Fireball || (mover is Bobby && direction == default);


		public void OnEnter(Mover mover)
		{
			if (mover is not Bobby) return;

			mover.transform.parent = transform;
			mover.transform.localPosition = new(0, 0.4f);
			if (wood)
			{
				MoveWoodBalances(wood);
				wood = null;
				return;
			}

			direction = mover.direction;
			Move().Forget();
		}


		public bool CanExit(Mover mover) => mover is not Bobby || direction == default;


		public void OnExit(Mover mover)
		{
			if (mover is not Bobby) return;

			mover.transform.parent = null;
			mover.transform.position = transform.position;
		}


		private static int count;
		private static bool check;
		private static readonly List<LotusLeaf> inertias = new();
		private static readonly Dictionary<LotusLeaf, WaterFlow> balances = new();
		private static readonly List<LotusLeaf> tmp = new();
		private static async UniTask CheckBalancesAndInertias()
		{
			var token = PlayGround.Token;
			while (true)
			{
				await UniTask.NextFrame();
				if (token.IsCancellationRequested || count == 0 || (balances.Count == 0 && inertias.Count == 0)) return;

				if (!check) continue;

				check = false;

				// Kiểm tra balances
				tmp.Clear();
				foreach (var leaf_flow in balances)
					if (leaf_flow.Key.CanMove(leaf_flow.Value.direction))
					{
						leaf_flow.Key.direction = leaf_flow.Value.direction;
						tmp.Add(leaf_flow.Key);
					}

				foreach (var leaf in tmp) leaf.Move().Forget();

				// Kiểm tra inertias
				tmp.Clear();
				tmp.AddRange(inertias);
				foreach (var leaf in tmp) leaf.Move().Forget();
			}
		}


		private static readonly Dictionary<LotusLeaf, (WaterFlow, Wood)> woodBalances = new();
		private static Wood wood;
		public static void OnWoodDeleted(Wood wood, Mover mover)
		{
			if (mover is Bobby && Platform.Peek(mover.transform.position + mover.direction) is LotusLeaf lotusLeaf
				&& woodBalances.ContainsKey(lotusLeaf))
			{
				// Bobby đang đi đến lá sen trong hệ cân bằng động, sẽ xử lý khi Bobby tới lá sen 
				LotusLeaf.wood = wood;
				return;
			}

			MoveWoodBalances(wood);
		}


		private static void MoveWoodBalances(Wood wood)
		{
			tmp.Clear();
			foreach (var leaf_flow_wood in woodBalances)
				if (leaf_flow_wood.Value.Item2 == wood)
				{
					leaf_flow_wood.Key.direction = leaf_flow_wood.Value.Item1.direction;
					tmp.Add(leaf_flow_wood.Key);
				}

			foreach (var leaf in tmp.Random()) leaf.Move().Forget();
		}


		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void Init() =>
			PlayGround.onAwake += () =>
			{
				task = UniTask.CompletedTask;
				count = 0;
				check = false;
				inertias.Clear();
				balances.Clear();
				woodBalances.Clear();
				wood = null;
			};
	}
}