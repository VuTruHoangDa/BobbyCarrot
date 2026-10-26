using BobbyCarrot.Movers;
using RotaryHeart.Lib.SerializableDictionary;
using System.Collections.Generic;
using UnityEngine;


namespace BobbyCarrot.Platforms
{
	[CreateAssetMenu(fileName = "ConveyorButton", menuName = "Platforms/ConveyorButton")]
	public sealed class ConveyorButton : Platform
	{
		private static readonly List<ConveyorButton> buttons = new();
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void Init() => PlayGround.onAwake += () => buttons.Clear();


		[SerializeField] private SerializableDictionaryBase<bool, Sprite> sprites;
		private bool on;

		protected override Platform New()
		{
			var p = Instantiate(this);
			p.on = id == 161;
			buttons.Add(p);
			return p;
		}


		public override bool CanEnter(Mover mover) => mover is not IPlatform;


		public override void OnEnter(Mover mover)
		{
			if (on || mover is Flyer or Fireball) return;

			foreach (var button in buttons) button.sprite = sprites[button.on = !button.on];
			Conveyor.ChangeStates();
		}
	}
}