using Cysharp.Threading.Tasks;
using RotaryHeart.Lib.SerializableDictionary;
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
		private UniTask task;
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
					while (direction != default && CanMove())
					{
						if (!await Move()) return;

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


		[field: SerializeField] public float originalSpeed { get; private set; }
		private void OnEnable()
		{
			direction = default;
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