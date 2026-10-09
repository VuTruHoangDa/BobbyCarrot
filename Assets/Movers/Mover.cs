using BobbyCarrot.Platforms;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;


namespace BobbyCarrot.Movers
{
	[RequireComponent(typeof(SpriteRenderer))]
	public abstract class Mover : MonoBehaviour
	{
		#region Show singleton
		public static T Show<T>(Vector3 position, Vector3 direction) where T : Mover
		{
#if DEBUG
			if (typeof(T) is IPlatform) throw new Exception($"{typeof(T)} phải được sinh thông qua Platform !");
#endif
			if (movers.TryGetValue(typeof(T), out var mover))
			{
				mover.transform.position = position;
				if (mover is IPlayer p) p.face = direction; else mover.direction = direction;
#if DEBUG
				if (mover.gameObject.activeSelf) throw new Exception($"{mover} đang bật, không thể bật thêm lần nữa !");
#endif
				mover.gameObject.SetActive(true);
				Camera.Focus(mover);
				return mover as T;
			}

			mover = Addressables.InstantiateAsync($"Assets/Movers/Prefab/{typeof(T).Name}.prefab",
				position, Quaternion.identity).WaitForCompletion().GetComponent<T>();
#if DEBUG
			if (mover.enabled) throw new Exception($"Phải tắt component của {mover} trong prefab !");
#endif
			if (mover is IPlayer p1) p1.face = direction; else mover.direction = direction;
			mover.enabled = true;
			Camera.Focus(mover);
			return mover as T;
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T Get<T>() where T : Mover =>
#if DEBUG
			typeof(T) is IPlatform ? throw new Exception($"{typeof(T)} phải được truy cập thông qua Platform !") :
#endif
			movers.TryGetValue(typeof(T), out var mover) ? mover as T : null;


		protected void Awake()
		{
			if (this is IPlatform) return;
			if (movers.TryGetValue(GetType(), out _)) throw new Exception($"{this} phải là Singleton !");
			movers[GetType()] = this;
		}


		private static readonly Dictionary<Type, Mover> movers = new();
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void Init() => PlayGround.onAwake += () => movers.Clear();
		#endregion


		/// <summary>
		/// Chú ý: không được thay đổi direction khi task Mover.Move() đang chạy<para/>
		/// Vì đang di chuyển thì mover chưa đi vào platform đích đến<br/>
		/// Khi vào đích đến thì platform có thể kiểm tra/thay đổi trạng thái mover<br/>
		/// direction == 0: đứng yên<br/>
		/// direction != 0: đang di chuyển
		/// </summary>
		public Vector3 direction { get; protected set; }
		public float speed;
		[SerializeField] private int delay;


		/// <summary>
		/// Chú ý: không được kiểm tra CanMove khi task Mover.Move() đang chạy !<para/>
		/// Vì đang di chuyển thì tọa độ không nguyên và mover chưa đi vào platform đích đến<br/>
		/// Khi mover vào đích đến thì trạng thái mover hoặc trạng thái bản đồ hoặc cả 2 có thể thay đổi<br/>
		/// => Kiểm tra CanMove ngay lúc này kết quả có thể khác với kiểm tra sau khi di chuyển
		/// </summary>
		protected virtual bool CanMove(Vector3 newDirection = default)
		{
			var pos = transform.position;
			return (
				// Nếu this là IPlatform (LotusLeaf, Cloud) thì kiểm tra platform ngay bên dưới this
				this is IPlatform ? Platform.Peek(pos, Platform.Peek(pos) as Mover == this ? 2 : 3).CanExit(this)
				: Platform.Peek(pos).CanExit(this)
					)
				&& Platform.Peek(pos + (newDirection != default ? newDirection : direction)).CanEnter(this);
		}


		/// <returns><see langword="true"/>: Kết thúc di chuyển bình thường<br/>
		/// <see langword="false"/>: Mover bị tắt hoặc PlayGround kết thúc
		///</returns>
		protected virtual async UniTask<bool> Move()
		{
			using var cts = CancellationTokenSource.CreateLinkedTokenSource(Token, PlayGround.Token);
			if (this is not IPlatform)
			{
				Platform.Peek(transform.position).OnExit(this);
				if (cts.IsCancellationRequested) return false;
			}

			IPlatform p = null;
			var pos = transform.position;
			if (this is IPlatform)
			{
				var top = Platform.Pop(pos);
				if (top as Mover != this)
				{
					Platform.Pop(pos);
					Platform.Push(top as Platform);
				}

				p = Platform.Peek(pos += direction);
				Platform.Push(pos, this);
			}
			else pos += direction;
			do
			{
				await UniTask.Delay(delay);
				if (cts.IsCancellationRequested) return false;
				transform.position = Vector3.MoveTowards(transform.position, pos, speed);
			} while (transform.position != pos);

			transform.position = pos;
			if (p != null) p.OnEnter(this); else Platform.Peek(pos).OnEnter(this);
			if (cts.IsCancellationRequested) return false;
			return true;
		}


		[field: SerializeField] public SpriteRenderer spriteRenderer { get; private set; }
		private CancellationTokenSource cts = new();
		public CancellationToken Token => cts.Token;
		protected void OnDisable()
		{
			cts.Cancel();
			cts.Dispose();
			cts = new();
		}
	}



	/// <summary>
	/// Người chơi hoặc CPU có thể điều khiển di chuyển
	/// </summary>
	public interface IPlayer
	{
		/// <summary>
		/// Hướng xoay mặt, không phải hướng di chuyển
		/// </summary>
		Vector3 face { get; set; }

		/// <summary>
		/// Người chơi/CPU điều khiển di chuyển, không xác định đích đến, nếu bị chặn thì dừng lại<para/>
		/// Chú ý: ngay sau khi cài dpad thì player có thể thoát khỏi platform<br/>
		/// => Trạng thái mover và bản đồ có thể thay đổi.
		/// </summary>
		Vector3 dpad { get; set; }

		float originalSpeed { get; }

		/// <summary>
		/// CPU di chuyển theo hướng direction và dừng lại khi tới dest hoặc bị chặn<para/>
		/// Chú ý: khi đang di chuyển nếu gọi Move với direction khác sẽ lỗi<br/>
		/// Nếu gọi Move khi đang di chuyển với dest mới với quãng đường mới lớn hơn thì di chuyển tới dest mới
		/// </summary>
		void Move(Vector3 direction, Vector3 dest);
	}
}