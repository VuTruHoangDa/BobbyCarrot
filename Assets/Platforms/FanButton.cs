using BobbyCarrot.Movers;
using Cysharp.Threading.Tasks;
using RotaryHeart.Lib.SerializableDictionary;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace BobbyCarrot.Platforms
{
	[CreateAssetMenu(fileName = "FanButton", menuName = "Platforms/FanButton")]
	public sealed class FanButton : Platform
	{
		private static readonly IReadOnlyDictionary<Color, List<FanButton>> buttons = new Dictionary<Color, List<FanButton>>
		{
			[Color.Yellow] = new List<FanButton>(),
			[Color.Red] = new List<FanButton>(),
			[Color.Green] = new List<FanButton>(),
			[Color.Violet] = new List<FanButton>()
		};

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void Init() =>
			PlayGround.onAwake += () =>
			{
				foreach (Color color in Enum.GetValues(typeof(Color))) buttons[color].Clear();
				listTurnOn.Clear();
			};


		private Color color;
		private bool on;
		[SerializeField] private SerializableDictionaryBase<Color, SerializableDictionaryBase<bool, Sprite>> sprites;
		public static readonly int Task_WaitTurningOn = "PinWheelButton.Create.WaitTurningOn".GetHashCode();
		private static readonly List<Color> listTurnOn = new();

		protected override Platform Create()
		{
			var p = base.Create() as FanButton;
			p.sprites = sprites;

			switch (id)
			{
				case 167:
					p.color = Color.Yellow;
					p.on = true;
					break;

				case 168:
					p.color = Color.Yellow;
					p.on = false;
					break;

				case 169:
					p.color = Color.Red;
					p.on = true;
					break;

				case 170:
					p.color = Color.Red;
					p.on = false;
					break;

				case 171:
					p.color = Color.Green;
					p.on = true;
					break;

				case 172:
					p.color = Color.Green;
					p.on = false;
					break;

				case 173:
					p.color = Color.Violet;
					p.on = true;
					break;

				case 174:
					p.color = Color.Violet;
					p.on = false;
					break;
			}

			buttons[p.color].Add(p);
			if (p.on)
			{
				if (listTurnOn.Count == 0) WaitTurningOn();
				if (!listTurnOn.Contains(p.color)) listTurnOn.Add(p.color);
			}

			return p;


			static async void WaitTurningOn()
			{
				PlayGround.tasks.Add(Task_WaitTurningOn);
				do await UniTask.Yield();
				while (PlayGround.tasks.Contains(Task_Platform_Init));

				foreach (var color in listTurnOn.Random())
				{
					foreach (var fan in Fan.fans[color].Random()) fan.ChangeState(true);
					await UniTask.Yield();
				}

				PlayGround.tasks.Remove(Task_WaitTurningOn);
			}
		}


		public override bool CanEnter(Mover mover) => mover is not IPlatform;


		public override void OnEnter(Mover mover)
		{
			if (mover is Flyer or Fireball) return;

			on = !on;
			foreach (var button in buttons[color]) button.sprite = sprites[color][button.on = on];
			foreach (var fan in Fan.fans[color].Random()) fan.ChangeState(on);
			if (!on) Cloud.OnTurnOffFans();
		}
	}
}