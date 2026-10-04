using BobbyCarrot.Movers;
using System;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;


namespace BobbyCarrot.Platforms
{
	[CreateAssetMenu(fileName = "Item", menuName = "Platforms/Item")]
	public sealed class Item : Platform
	{
		public enum Type : ushort
		{
			Shoe = 188, Glass = 189, Key = 185, YellowMap = 183, BlueMap = 184, Speaker = 186, MusicNote = 187,
			Seed = 143, Shovel = 191, Gas = 125, Kite = 83, GoldenCarrot = 86, Coin = 88, Gun = 78
		}
		private Type type;

		public static readonly ReadOnlyArray<ushort> itemIDs;
		static Item()
		{
			var types = Enum.GetValues(typeof(Type)) as Type[];
			var array = new ushort[types.Length];
			itemIDs = new(array);
			for (int i = 0; i < types.Length; ++i) array[i] = (ushort)types[i];
		}


		protected override Platform Create()
		{
			var p = base.Create() as Item;
			p.type = (Type)id;

			return p;
		}


		public override bool CanEnter(Mover mover) => mover is not IPlatform;


		public override void OnEnter(Mover mover)
		{
			if (mover is Flyer or Fireball) return;

			++PlayGround.items[type];
			Pop(position);
		}
	}
}