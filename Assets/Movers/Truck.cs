using Cysharp.Threading.Tasks;
using RotaryHeart.Lib.SerializableDictionary;
using UnityEngine;


namespace BobbyCarrot.Movers
{
	[RequireComponent(typeof(Animator))]
	public sealed class Truck : Mover, IGamepadListener
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
					while (dpad != default && CanMove(direction = dpad))
					{
						animator.runtimeAnimatorController = anims[direction];
						if (!await Move()) return;
					}
				}
			}
		}


		private Vector3 _direction;
		public override Vector3 direction
		{
			get => _direction;
			protected set => animator.runtimeAnimatorController = anims[_direction = value];
		}


		[SerializeField] private SerializableDictionaryBase<Vector3, RuntimeAnimatorController> anims;
		[SerializeField] private Animator animator;

		private void OnEnable()
		{
			Main.AddListener(this);
		}


		private new void OnDisable()
		{
			base.OnDisable();
			Main.RemoveListener(this);
		}
	}
}