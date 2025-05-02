using UnityEngine;


namespace BobbyCarrot.Movers
{
	[RequireComponent(typeof(Animator))]
	public sealed class Fireball : Mover
	{
		/// <summary>
		/// Hướng di chuyển ở bước tiếp theo: Up, Right, Down, Left, Zero (Zero=hủy/dừng)<br/>
		/// Xử lý xong thì dpad == moverDirection<para/>
		/// </summary>
		public Vector3 input;


		private async void OnEnable()
		{
			input = direction;
			while (CanMove())
			{
				if (!await Move()) return;
				if (input != direction)
				{
					input.CheckValidDpad();
					if ((direction = input) == default) break;
				}
			}

			gameObject.SetActive(false);
		}
	}
}