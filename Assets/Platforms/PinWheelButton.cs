using BobbyCarrot.Movers;
using Cysharp.Threading.Tasks;
using RotaryHeart.Lib.SerializableDictionary;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace BobbyCarrot.Platforms
{
	[CreateAssetMenu(fileName = "PinWheelButton", menuName = "Platforms/PinWheelButton")]
	public sealed class PinWheelButton : Platform
	{
		private static readonly IReadOnlyDictionary<Color, List<PinWheelButton>> dict
			= new Dictionary<Color, List<PinWheelButton>>
			{
				[Color.Yellow] = new List<PinWheelButton>(),
				[Color.Red] = new List<PinWheelButton>(),
				[Color.Green] = new List<PinWheelButton>(),
				[Color.Violet] = new List<PinWheelButton>()
			};

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void Init()
		{
			PlayGround.onAwake += () =>
			{
				foreach (Color color in Enum.GetValues(typeof(Color))) dict[color].Clear();
				listTurnOn.Clear();
			};
		}


		private Color color;
		private bool on;
		[SerializeField] private SerializableDictionaryBase<Color, SerializableDictionaryBase<bool, Sprite>> sprites;

		protected override Platform Create()
		{
			var p = base.Create() as PinWheelButton;
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

			p.sprite = sprites[p.color][p.on];
			dict[p.color].Add(p);
			if (p.on)
			{
				if (listTurnOn.Count == 0) WaitTurningOn();
				if (!listTurnOn.Contains(p.color)) listTurnOn.Add(p.color);
			}

			return p;
		}


		public static readonly new int TASK_ID = "PinWheelButton.WaitTurningOn".GetHashCode();

		private static readonly List<Color> listTurnOn = new();
		private static async void WaitTurningOn()
		{
			var token = PlayGround.Token;
			PlayGround.taskList.Add(TASK_ID);
			while (!token.IsCancellationRequested
				&& PlayGround.taskList.Contains(Platform.TASK_ID)) await UniTask.Yield();
			if (token.IsCancellationRequested) return;

			do
			{
				var color = listTurnOn[UnityEngine.Random.Range(0, listTurnOn.Count)];
				listTurnOn.Remove(color);
				foreach (var pinWheel in PinWheel.dict[color].Random())
					pinWheel.ChangeState(true);
				await UniTask.Yield();
				if (token.IsCancellationRequested) return;
			} while (listTurnOn.Count != 0);
			PlayGround.taskList.Remove(TASK_ID);
		}


		public override bool CanEnter(Mover mover) => mover is not LotusLeaf && mover is not Cloud;


		public override void OnEnter(Mover mover)
		{
			if (mover is Flyer || mover is Fireball) return;

			foreach (var button in dict[color])
				button.sprite = sprites[color][button.on = !button.on];

			foreach (var pinWheel in PinWheel.dict[color].Random())
				pinWheel.ChangeState(on);
		}
	}
}