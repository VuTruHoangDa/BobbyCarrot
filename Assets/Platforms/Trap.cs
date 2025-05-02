using BobbyCarrot.Movers;
using Cysharp.Threading.Tasks;
using RotaryHeart.Lib.SerializableDictionary;
using UnityEngine;


namespace BobbyCarrot.Platforms
{
	[CreateAssetMenu(fileName = "Trap", menuName = "Platforms/Trap")]
	public sealed class Trap : Platform
	{
		[SerializeField] private SerializableDictionaryBase<bool, Sprite> sprites;
		private bool on;

		protected override Platform Create()
		{
			var p = base.Create() as Trap;
			p.sprites = sprites;
			p.sprite = sprites[p.on = id == 175];
			return p;
		}


		public override bool CanEnter(Mover mover) => mover is not LotusLeaf and not Cloud;


		public override void OnEnter(Mover mover)
		{
			if (on && mover is Bobby b) b.Die();
		}


		public override void OnExit(Mover mover)
		{
			if (mover is Bobby) sprite = sprites[on = true];
		}
	}
}