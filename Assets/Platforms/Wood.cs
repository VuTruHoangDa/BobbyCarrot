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

			// Che đậy lá sen nếu có
			if (Peek(position) is LotusLeaf leaf) leaf.spriteRenderer.sortingLayerID = 0;

			return p;
		}


		public override bool CanEnter(Mover mover) => mover is not IPlatform;


		public override async void OnExit(Mover mover)
		{
			if (mover is Flyer or Fireball) return;

			Pop(position);
			UI = Instantiate(UI, position, Quaternion.identity);
			UniTask.Delay((int)(UI.runtimeAnimatorController.animationClips[0].length * 1000) + 500)
				.ContinueWith(() =>
				{
					if (UI) Destroy(UI.gameObject);
				}).Forget();

			// Hiện lá sen nếu có
			if (Peek(position) is LotusLeaf leaf)
			{
				leaf.spriteRenderer.sortingLayerID = Util.Layer_Mover;
				leaf.spriteRenderer.sortingOrder = -1;
			}

			LotusLeaf.OnWoodDeleted(this, mover);
			Cloud.OnWoodDeleted(this, mover);
		}
	}
}