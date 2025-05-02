using BobbyCarrot.Movers;
using Cysharp.Threading.Tasks;
using UnityEngine;


namespace BobbyCarrot.Platforms
{
	[CreateAssetMenu(fileName ="Carrot", menuName ="Platforms/Carrot")]
	public sealed class Carrot : Platform
	{
		private enum Type
		{
			Hole, Carrot, Leaf
		}
		private Type type;

		[SerializeField] private Sprite hole;
		[SerializeField] private AnimationData carrot, leaf;

		protected override Platform Create()
		{
			var p = base.Create() as Carrot;
			p.hole = hole;
			p.carrot = carrot;
			p.leaf = leaf;

			switch (id)
			{
				case 136:
					p.type = Type.Leaf;
					p.animationData = leaf;
					break;

				case 137:
					p.type = Type.Hole;
					break;

				default:
					p.type = Type.Carrot;
					p.animationData = carrot;
					break;
			}

			return p;
		}


		public override bool CanEnter(Mover mover) =>
			mover is not LotusLeaf and not Cloud
			&& (mover is Flyer or Fireball || type != Type.Leaf || mover is Truck);


		public override void OnEnter(Mover mover)
		{
			if (mover is Flyer or Fireball) return;

			switch (type)
			{
				case Type.Leaf:
					type = Type.Carrot;
					animationData = carrot;

					// UI cắt lá

					break;

				case Type.Carrot:
					type = Type.Hole;
					animationData = default;
					sprite = hole;
					++PlayGround.carrot;
					break;

				default: return;
			}
		}
	}
}