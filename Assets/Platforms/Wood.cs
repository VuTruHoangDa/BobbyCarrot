using BobbyCarrot.Movers;
using Cysharp.Threading.Tasks;
using UnityEngine;


namespace BobbyCarrot.Platforms
{
	[CreateAssetMenu(fileName = "Wood", menuName = "Platforms/Wood")]
	public sealed class Wood : Platform
	{
		protected override Platform Create()
		{
			var p = base.Create() as Wood;
			p.anim = anim;

			// Che đậy lá sen nếu có
			if (Peek(position) is LotusLeaf leaf) leaf.spriteRenderer.sortingLayerID = 0;

			return p;
		}


		public override bool CanEnter(Mover mover) => mover is not IPlatform;


		[SerializeField] private Animator anim;
		public override void OnExit(Mover mover)
		{
			if (mover is Flyer or Fireball) return;

			Pop(position);
			anim = Instantiate(anim, position, Quaternion.identity);
			UniTask.Delay((int)(anim.runtimeAnimatorController.animationClips[0].length * 1000) + 500)
				.ContinueWith(() =>
				{
					if (anim) Destroy(anim.gameObject);
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