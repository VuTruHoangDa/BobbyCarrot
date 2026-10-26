using BobbyCarrot.Movers;
using RotaryHeart.Lib.SerializableDictionary;
using System.Collections.Generic;
using UnityEngine;


namespace BobbyCarrot.Platforms
{
	[CreateAssetMenu(fileName = "WaterButton", menuName = "Platforms/WaterButton")]
	public sealed class WaterButton : Platform
	{
		private static readonly List<WaterButton> buttons = new();

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void Init() => PlayGround.onAwake += () => buttons.Clear();


		[SerializeField] private SerializableDictionaryBase<bool, Sprite> sprites;
		private bool on;
		protected override Platform New()
		{
			var p = Instantiate(this);
			p.on = id == 165;
			buttons.Add(p);
			return p;
		}


		public override bool CanEnter(Mover mover) => mover is not IPlatform;


		public override void OnEnter(Mover mover)
		{
			if (on || mover is not Bobby) return;

			foreach (var button in buttons) button.sprite = sprites[button.on = !button.on];
			WaterFlow.ChangeStates();
		}
	}
}