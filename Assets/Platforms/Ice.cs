using BobbyCarrot.Movers;
using Cysharp.Threading.Tasks;
using UnityEngine;


namespace BobbyCarrot.Platforms
{
	[CreateAssetMenu(fileName = "Ice", menuName = "Platforms/Ice")]
	public sealed class Ice : Platform
	{
		[SerializeField] private AnimationData anim;
		[SerializeField] private int duration;

		protected override Platform Create()
		{
			var p = base.Create() as Ice;
			p.anim = anim;
			p.duration = duration;
			return p;
		}


		public override bool CanEnter(Mover mover) =>
			mover is Flyer or Fireball;


		public override async void OnEnter(Mover mover)
		{
			if (mover is Flyer || animationData.animatedSprites.Length != 0) return;

			animationData = anim;
			var token = PlayGround.Token;
			await UniTask.Delay(duration);
			if (!token.IsCancellationRequested) Pop(index);
		}
	}
}