using BobbyCarrot.Movers;
using Cysharp.Threading.Tasks;
using UnityEngine;


namespace BobbyCarrot.Platforms
{
	[CreateAssetMenu(fileName = "Wood", menuName = "Platforms/Wood")]
	public sealed class Wood : Platform
	{
		[SerializeField] private Animator UI;


		protected override Platform Create()
		{
			var p = base.Create() as Wood;
			p.UI = UI;
			return p;
		}


		public override bool CanEnter(Mover mover) => mover is not LotusLeaf and not Cloud;


		public override void OnExit(Mover mover)
		{
			if (mover is Flyer or Fireball) return;

			Pop(index);
			UI = Instantiate(UI, index, Quaternion.identity);
			UniTask.Delay((int)(UI.runtimeAnimatorController.animationClips[0].length * 1000) + 500)
				.ContinueWith(() =>
				{
					if (UI) Destroy(UI.gameObject);
				}).Forget();
		}
	}
}