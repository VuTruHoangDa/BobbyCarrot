using BobbyCarrot.Platforms;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;


namespace BobbyCarrot.Movers
{
	public sealed class LotusLeaf : Mover, IPlatform
	{
		public async void Move(Vector3 direction)
		{
			if (this.direction != default) throw new System.Exception("Lá sen đang trong bước di chuyển, không thể thay đổi moverDirection !");
			direction.CheckValidDpad();
			if ((this.direction = direction) != default) await Move();
		}


		protected override async UniTask<bool> Move()
		{
			if (dict.ContainsKey(this)) dict.Remove(this);
			while (CanMove()) if (!await base.Move()) return false;
			direction = default;

			// Kiểm tra tại lá sen có dòng nước (WaterFlow) ?
			// Nếu có thì lá sen đang "cân bằng động" => Kiểm tra lá sen mỗi frame
			var pos = transform.position;
			Platform.Pop(pos);
			var waterFlow = Platform.Peek(pos) as WaterFlow;
			Platform.Push(pos, this);
			if (!waterFlow) return true;

			dict[this] = waterFlow;
			if (dict.Count == 1) CheckSpecialLeafs();
			return true;
		}


		public bool CanEnter(Mover mover) =>
			mover is Flyer or Fireball || (mover is Bobby && direction == default);


		public async void OnEnter(Mover mover)
		{
			if (mover is not Bobby) return;

			mover.transform.parent = transform;
			direction = mover.direction;
			await Move();
		}


		public bool CanExit(Mover mover) => mover is not Bobby || direction == default;


		public void OnExit(Mover mover) => mover.transform.parent = null;


		#region Kiểm tra các lá sen cân bằng động
		private static readonly Dictionary<LotusLeaf, WaterFlow> dict = new();
		private static readonly List<LotusLeaf> tmp = new();
		private static async void CheckSpecialLeafs()
		{
			var token = PlayGround.Token;
			while (true)
			{
				await UniTask.DelayFrame(1);
				if (token.IsCancellationRequested || dict.Count == 0) return;

				tmp.Clear();
				foreach (var kvp in dict)
					if (kvp.Key && kvp.Key.CanMove(kvp.Value.direction))
					{
						kvp.Key.direction = kvp.Value.direction;
						tmp.Add(kvp.Key);
					}

				if (tmp.Count == 0) continue;
				foreach (var leaf in tmp) leaf.Move().Forget();
			}
		}


		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void Init()
		{
			PlayGround.onAwake += () => dict.Clear();
		}
		#endregion
	}
}