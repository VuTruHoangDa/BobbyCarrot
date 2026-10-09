using BobbyCarrot.Movers;
using Cysharp.Threading.Tasks;
using UnityEngine;


namespace BobbyCarrot.Platforms
{
	[CreateAssetMenu(fileName = "Ice", menuName = "Platforms/Ice")]
	public sealed class Ice : Platform
	{
		protected override Platform Create()
		{
			var p = base.Create() as Ice;
			p.anim = anim;

			return p;
		}


		public override bool CanEnter(Mover mover) => mover is Flyer or Fireball;


		[SerializeField] private Animator anim;
		public override async void OnEnter(Mover mover)
		{
			if (mover is Flyer) return;

			Pop(position);
			anim = Instantiate(anim, position, Quaternion.identity);
			UniTask.Delay((int)(anim.runtimeAnimatorController.animationClips[0].length * 1000) + 500)
				.ContinueWith(() =>
				{
					if (anim) Destroy(anim.gameObject);
				}).Forget();
		}
	}
}