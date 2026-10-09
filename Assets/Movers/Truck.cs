using Cysharp.Threading.Tasks;
using RotaryHeart.Lib.SerializableDictionary;
using System;
using System.Threading;
using UnityEngine;


namespace BobbyCarrot.Movers
{
	[RequireComponent(typeof(Animator))]
	public sealed class Truck : Mover, IPlayer
	{
		[SerializeField] private SerializableDictionaryBase<Vector3, RuntimeAnimatorController> anims;
		[SerializeField] private Animator animator;
		private Vector3 _face;
		public Vector3 face
		{
			get => _face;

			set => animator.runtimeAnimatorController = anims[_face = value];
		}


		private Vector3? newDir;
		private UniTask taskDpad;
		private CancellationTokenSource cancelDpad = new();
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
					var token = cancelDpad.Token;
					while (direction != default && CanMove())
					{
						if (!await Move() || token.IsCancellationRequested) return;

						if (newDir != null)
						{
							direction = newDir.Value;
							newDir = null;
						}
					}

					direction = default;
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

			this.direction = face = direction;
			this.dest = dest;
			do
			{
				if (!CanMove()) break;
				if (!await Move()) return;
			} while (transform.position != this.dest);

			this.direction = this.dest = default;
		}


		[field: SerializeField] public float originalSpeed { get; private set; }
		private void OnEnable()
		{
			direction = dest = default;
			speed = originalSpeed;
			Main.Register(this);
		}


		private new void OnDisable()
		{
			base.OnDisable();
			Main.Unregister(this);
		}
	}
}