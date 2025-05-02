using BobbyCarrot.Movers;
using RotaryHeart.Lib.SerializableDictionary;
using System.Collections.Generic;
using UnityEngine;


namespace BobbyCarrot.Platforms
{
	[CreateAssetMenu(fileName = "MazeButton", menuName = "Platforms/MazeButton")]
	public sealed class MazeButton : Platform
	{
		private static readonly List<MazeButton> buttons = new();
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void Init()
		{
			PlayGround.onAwake += () => buttons.Clear();
		}


		[SerializeField] private SerializableDictionaryBase<bool, Sprite> sprites;
		private bool on;

		protected override Platform Create()
		{
			var p = base.Create() as MazeButton;
			p.sprites = sprites;
			p.sprite = sprites[p.on = id == 163];
			buttons.Add(p);
			return p;
		}


		public override bool CanEnter(Mover mover) =>
			mover is not LotusLeaf and not Cloud;


		public override void OnEnter(Mover mover)
		{
			if (on || mover is not Bobby) return;

			foreach (var button in buttons) button.sprite = sprites[button.on = !button.on];
			Maze.ChangeState();
		}
	}
}