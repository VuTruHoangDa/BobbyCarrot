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
		private UniTask task;
		private Vector3? newDir;
		private Vector3 _dpad;
		public Vector3 dpad
		{
			get => _dpad;

			set
			{
				_dpad = value;
				if (value != default) face = value;
				if (task.isRunning()) newDir = value;
				else
				{
					direction = value;
					newDir = null;
					if (value != default) task = Check();
				}


				async UniTask Check()
				{
					cancelRelax.Cancel();
					cancelRelax.Dispose();
					cancelRelax = new();
					var pos = transform.position;
					while (direction != default && CanMove())
					{
						if (animator.runtimeAnimatorController != walkAnims[direction])
							animator.runtimeAnimatorController = walkAnims[direction];
						if (Platform.Peek(pos += direction) is Mover m) moverPlatform = m as IPlatform;
						if (!await Move()) return;

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


		[SerializeField] private SerializableDictionaryBase<Vector3, Sprite> sprites;
		[SerializeField] private RuntimeAnimatorController relaxAnim;
		private CancellationTokenSource cancelRelax = new();
		[SerializeField] private int delayRelax;
		private async void Idle()
		{
			animator.runtimeAnimatorController = null;
			direction = default;
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
		=> (moverPlatform != null ? moverPlatform.CanExit(this) : Platform.Peek(transform.position.ToVector3Int()).CanExit(this))
				&& Platform.Peek(transform.position.ToVector3Int() + (newDirection != default ? newDirection : direction)).CanEnter(this);
	}
}