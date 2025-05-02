using Cysharp.Threading.Tasks;
using RotaryHeart.Lib.SerializableDictionary;
using System;
using System.Threading;
using UnityEngine;


namespace BobbyCarrot.Movers
{
	[RequireComponent(typeof(Animator))]
	public sealed class Bobby : Mover, IGamepadListener
	{
		private UniTask task;
		private Vector3 _dpad;
		public Vector3 dpad
		{
			get => _dpad;
			set
			{
				value.CheckValidDpad();
				if (value == _dpad) return;

				_dpad = value;
				if (task.isRunning()) return;

				(task = Check()).Forget();
				async UniTask Check()
				{
					cancelRelax.Cancel();
					cancelRelax.Dispose();
					cancelRelax = new();
					while (dpad != default && CanMove(direction = dpad))
					{
						animator.runtimeAnimatorController = walkAnims[direction];
						if (!await Move()) return;
					}

					Idle();
				}
			}
		}


		private CancellationTokenSource cancelRelax = new();
		[SerializeField] private int delayRelax;
		private async void Idle()
		{
			animator.runtimeAnimatorController = null;
			spriteRenderer.sprite = sprites[direction];
			using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancelRelax.Token, Token, PlayGround.Token);
			if (!await UniTask.Delay(delayRelax, cancellationToken: cts.Token).SuppressCancellationThrow())
				animator.runtimeAnimatorController = relaxAnim;
		}


		public async void Die()
		{
			throw new NotImplementedException();
		}


		[SerializeField] private Animator animator;
		[SerializeField] private SerializableDictionaryBase<Vector3, Sprite> sprites;
		[SerializeField] private SerializableDictionaryBase<Vector3, RuntimeAnimatorController> walkAnims, shovelAnims;
		[SerializeField] private RuntimeAnimatorController relaxAnim, dieAnim;

		private void OnEnable()
		{
			Idle();
			Main.AddListener(this);
		}


		private new void OnDisable()
		{
			base.OnDisable();
			Main.RemoveListener(this);
		}
	}
}