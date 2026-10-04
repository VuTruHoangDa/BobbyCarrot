using BobbyCarrot;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;


public class Test : MonoBehaviour
{
	UniTask t;
	private void Start()
	{
		t = A();
	}


	async UniTask A()
	{
		await UniTask.Delay(5000);
		int i = 0;
		int a = 1 / i;
	}


	private async void Update()
	{
		if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
			print(t.isRunning());

	}
}