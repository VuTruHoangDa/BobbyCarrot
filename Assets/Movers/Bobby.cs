using BobbyCarrot.Platforms;
using Cysharp.Threading.Tasks;
using RotaryHeart.Lib.SerializableDictionary;
using System;
using System.Threading;
using UnityEngine;


namespace BobbyCarrot.Movers
{
	[RequireComponent(typeof(Animator))]
	public sealed class Bobby : Mover, IPlayer
	{
		[SerializeField] private Animator animator;
		public Vector3 face { get; set; }
		[field: SerializeField] public float originalSpeed { get; private set; }


		[SerializeField] private SerializableDictionaryBase<Vector3, RuntimeAnimatorController> walkAnims, shovelAnims;
		private UniTask taskDpad;
		private CancellationTokenSource cancelDpad = new();
		private Vector3? newDir;
		private Vector3 _dpad;
		public Vector3 dpad
		{
			get => _dpad;

			set
			{
				_dpad = value;
				if (value != default) face = value;
				if (taskDpad.isRunning()) newDir = value;
				else
				{
					direction = value;
					newDir = null;
					if (value != default) taskDpad = Check();
				}


				async UniTask Check()
				{
					cancelRelax.Cancel();
					cancelRelax.Dispose();
					cancelRelax = new();
					var pos = transform.position;
					var token = cancelDpad.Token;
					while (direction != default && CanMove())
					{
						if (animator.runtimeAnimatorController != walkAnims[direction])
							animator.runtimeAnimatorController = walkAnims[direction];
						moverPlatform = Platform.Peek(pos += direction) as Mover as IPlatform;
						if (!await Move() || token.IsCancellationRequested) return;

						if (newDir != null)
						{
							direction = newDir.Value;
							newDir = null;
						}
					}

					Idle();
				}
			}
		}


		private Vector3 dest;
		public async void Move(Vector3 direction, Vector3 dest)
		{
#if DEBUG
			if (direction == default)
				throw new ArgumentException("direction không hợp lệ !");
			if (this.dest != default && this.direction != direction)
				throw new ArgumentException("Mover đang di chuyển bằng hàm Move(), không thể đổi hướng !");
			if (dest == transform.position)
				throw new ArgumentException("dest trùng với toạ độ mover, không thể di chuyển !");
#endif
			if (this.dest != default)
			{
				if ((dest - transform.position).sqrMagnitude > (this.dest - transform.position).sqrMagnitude)
					this.dest = dest;
				return;
			}

			if (taskDpad.isRunning())
			{
				cancelDpad.Cancel();
				cancelDpad.Dispose();
				cancelDpad = new();
			}

			cancelRelax.Cancel();
			cancelRelax.Dispose();
			cancelRelax = new();
			animator.runtimeAnimatorController = walkAnims[this.direction = face = direction];
			this.dest = dest;
			var pos = transform.position;
			do
			{
				if (!CanMove()) break;

				moverPlatform = Platform.Peek(pos += direction) as Mover as IPlatform;
				if (!await Move()) return;
			} while (pos != this.dest);

			Idle();
		}


		[SerializeField] private SerializableDictionaryBase<Vector3, Sprite> sprites;
		[SerializeField] private RuntimeAnimatorController relaxAnim;
		private CancellationTokenSource cancelRelax = new();
		[SerializeField] private int delayRelax;
		private async void Idle()
		{
			animator.runtimeAnimatorController = null;
			direction = dest = default;
			spriteRenderer.sprite = sprites[face];
			using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancelRelax.Token, Token, PlayGround.Token);
			if (!await UniTask.Delay(delayRelax, cancellationToken: cts.Token).SuppressCancellationThrow())
				animator.runtimeAnimatorController = relaxAnim;
		}


		[SerializeField] private RuntimeAnimatorController dieAnim;
		public async void Die()
		{
			throw new NotImplementedException();
		}


		private void OnEnable()
		{
			Idle();
			moverPlatform = null;
			speed = originalSpeed;
			Main.Register(this);
		}


		private new void OnDisable()
		{
			base.OnDisable();
			Main.Unregister(this);
		}


		private IPlatform moverPlatform;
		/// <summary>
		/// Nếu Bobby đang đứng trên lá sen/mây thì kiểm tra lá sen/mây.CanExit
		/// </summary>
		protected override bool CanMove(Vector3 newDirection = default)
		=> (moverPlatform != null ? moverPlatform.CanExit(this) : Platform.Peek(transform.position).CanExit(this))
				&& Platform.Peek(transform.position + (newDirection != default ? newDirection : direction)).CanEnter(this);
	}
}