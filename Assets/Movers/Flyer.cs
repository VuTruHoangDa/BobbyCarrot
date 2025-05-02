using RotaryHeart.Lib.SerializableDictionary;
using UnityEngine;


namespace BobbyCarrot.Movers
{
	public sealed class Flyer : Mover
	{
		[SerializeField] private SerializableDictionaryBase<Vector3, Sprite> sprites;
		private async void OnEnable()
		{
			spriteRenderer.sprite = sprites[direction];
			while (CanMove()) if (!await Move()) return;
			gameObject.SetActive(false);
			Show<Bobby>(transform.position, direction);
		}
	}
}