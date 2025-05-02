using BobbyCarrot.Platforms;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;


namespace BobbyCarrot.Movers
{
	[RequireComponent(typeof(SpriteRenderer))]
	public abstract class Mover : MonoBehaviour
	{
		/// <summary>
		/// Hướng nhìn, hướng di chuyển: <para/>
		/// moverDirection == Zero: đứng yên<br/>
		/// moverDirection != Zero: đứng yên xoay mặt hoặc đang di chuyển
		/// </summary>
		public virtual Vector3 direction { get; protected set; }
		public float speed;
		[SerializeField] private int delay;


		public bool CanMove(Vector3? newDirection = null)
			=> (this is IPlatform || Platform.Peek(transform.position).CanExit(this))
				&& Platform.Peek(transform.position + (newDirection != null ? newDirection.Value : direction)).CanEnter(this);


		/// <returns><see langword="true"/>: Kết thúc di chuyển bình thường<br/>
		/// <see langword="false"/>: Mover bị hủy hoặc PlayGround kết thúc hoặc Platform chiếm quyền điều khiển Mover
		///</returns>
		protected virtual async UniTask<bool> Move()
		{
			var pos = transform.position;
			using var cts = CancellationTokenSource.CreateLinkedTokenSource(Token, PlayGround.Token);
			float originalSpeed = speed;

			if (this is not IPlatform)
			{
				Platform.Peek(pos).OnExit(this);
				if (cts.IsCancellationRequested || speed <= 0) return false;
			}

			IPlatform p = null;
			if (this is IPlatform)
			{
				Platform.Pop(pos);
				pos += direction;
				p = Platform.Peek(pos);
				Platform.Push(pos, this as IPlatform);
			}
			else pos += direction;

			while (transform.position != pos)
			{
				transform.position = Vector3.MoveTowards(transform.position, pos, speed);
				await UniTask.Delay(delay);
				if (cts.IsCancellationRequested) return false;
			}
			transform.position = pos;

			(p ?? Platform.Peek(pos)).OnEnter(this);
			if (cts.IsCancellationRequested || speed <= 0) return false;

			speed = originalSpeed;
			return true;
		}


		[SerializeField]
		[HideInInspector]
		protected SpriteRenderer spriteRenderer;
		protected void Reset()
		{
			spriteRenderer = GetComponent<SpriteRenderer>();
		}


		private CancellationTokenSource cts = new();
		public CancellationToken Token => cts.Token;
		protected void OnDisable()
		{
			cts.Cancel();
			cts.Dispose();
			cts = new();
		}


		#region Show singleton
		private static readonly Dictionary<Type, Mover> movers = new();
		public static T GetSingleton<T>() where T : Mover => movers[typeof(T)] as T;


		public static void Show<T>(in Vector3 position, in Vector3 direction) where T : Mover
		{
			if (typeof(T) is IPlatform) throw new Exception($"{typeof(T)} phải được sinh thông qua Platform !");

			var mover = movers[typeof(T)];
			if (mover)
			{
				mover.transform.position = position;
				mover.direction = direction;
				mover.gameObject.SetActive(true);
				return;
			}

			mover = Addressables.InstantiateAsync($"Assets/Movers/Prefab/{typeof(T).Name}.prefab",
				position, Quaternion.identity).WaitForCompletion().GetComponent<T>();
			mover.direction = direction;
			if (mover.enabled) throw new Exception($"Phải tắt component của {mover} trong prefab !");
			mover.enabled = true;
		}


		protected void Awake()
		{
			if (this is not IPlatform)
				movers[GetType()] = movers[GetType()] ? throw new Exception($"{this} phải là Singleton !") : this;
		}


		static Mover()
		{
			var t = typeof(Mover);
			foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
				foreach (var type in asm.GetTypes())
					if (type.BaseType == t && type.GetInterface("IPlatform") == null)
						movers[type] = null;
		}
		#endregion
	}



	public interface IGamepadListener
	{
		/// <summary>
		/// Gamepad hoặc CPU truyền dpad theo thời gian thực (frame)
		/// </summary>
		Vector3 dpad { get; set; }
	}
}