using BobbyCarrot.Movers;
using UnityEngine;


namespace BobbyCarrot.Platforms
{
	[CreateAssetMenu(fileName = "Carrot", menuName = "Platforms/Carrot")]
	public sealed class Carrot : Platform
	{
		private enum Type : ushort
		{
			Leaf = 136, Hole = 137, Carrot = 138
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
			p.animationData = (p.type = (Type)id) switch
			{
				Type.Carrot => carrot,
				Type.Leaf => leaf,
				_ => default
			};
			if (p.type != Type.Hole) ++PlayGround.totalCarrot;

			return p;
		}


		public override bool CanEnter(Mover mover) =>
			mover is not IPlatform
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
					// Nếu đủ Carrot thì 
					break;

				default: return;
			}
		}
	}
}