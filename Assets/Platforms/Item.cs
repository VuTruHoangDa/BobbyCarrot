using BobbyCarrot.Movers;
using Cysharp.Threading.Tasks;
using UnityEngine;


namespace BobbyCarrot.Platforms
{
	[CreateAssetMenu(fileName ="Item",menuName ="Platforms/Item")]
	public sealed class Item : Platform
	{
		public enum Type
		{
			Shoe, Glass, Key, YellowMap, BlueMap, Speaker, MusicNote, Bean, Shovel,
			Gas, Kite, GoldenCarrot, Coin, Gun
		}
		public Type type { get; private set; }


		protected override Platform Create()
		{
			var p = base.Create() as Item;
			p.type = id switch
			{
				183 => Type.YellowMap,
				184 => Type.BlueMap,
				185 => Type.Key,
				186 => Type.Speaker,
				187 => Type.MusicNote,
				188 => Type.Shoe,
				189 => Type.Glass,
				143 => Type.Bean,
				125 => Type.Gas,
				83 => Type.Kite,
				86 => Type.GoldenCarrot,
				88 => Type.Coin,
				78 => Type.Gun,
				191 => Type.Shovel,
				_ => throw new System.Exception()
			};

			return p;
		}


		public override bool CanEnter(Mover mover) => mover is not LotusLeaf and not Cloud;


		public override void OnEnter(Mover mover)
		{
			if (mover is Flyer or Fireball) return;

			++PlayGround.items[type];
			Pop(index);
		}
	}
}