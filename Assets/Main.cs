using BobbyCarrot.MapEditors;
using BobbyCarrot.Movers;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


namespace BobbyCarrot
{
	[DefaultExecutionOrder(-1)]	// Test
	public sealed class Main : MonoBehaviour
	{
		[SerializeField] private MapEditor editor;  // Test
		private Resolution fullRes;
		private void Awake()
		{
			foreach (var r in Screen.resolutions)
				if (r.width > fullRes.width || r.height > fullRes.height) fullRes = r;

			Screen.SetResolution(fullRes.width, fullRes.height, true);

			// Test
			map = new(editor.CreateMapFile());
			Destroy(editor.gameObject);
		}


		public static Map map { get; private set; }
		private static void NextMap()
		{
			throw new NotImplementedException();
		}


		#region Gamepad, Keyboard
		public static Vector3 dpad { get; private set; }
		public static event Action<Vector3> dpadChanged;
		private static readonly List<IPlayer> listeners = new();
		public static void Register(IPlayer listener)
		{
			if (listeners.Contains(listener)) return;

			listeners.Add(listener);
			if (listener.dpad != dpad) listener.dpad = dpad;
		}


		public static void Unregister(IPlayer listener)
		{
			if (!listeners.Contains(listener)) return;

			listeners.Remove(listener);
			if (listener.dpad != default) listener.dpad = default;
		}


		private void Update()
		{
			Vector3 d = default;

			// Keyboard
			if (Keyboard.current != null)
			{
				var k = Keyboard.current;
				if (k.f11Key.wasPressedThisFrame)
					if (Screen.fullScreen) Screen.fullScreenMode = FullScreenMode.MaximizedWindow;
					else Screen.SetResolution(fullRes.width, fullRes.height, true);

				d = k.upArrowKey.isPressed ? Vector3.up
					: k.rightArrowKey.isPressed ? Vector3.right
					: k.downArrowKey.isPressed ? Vector3.down
					: k.leftArrowKey.isPressed ? Vector3.left
					: d;
			}

			// Gamepad
			if (Gamepad.current != null)
			{
				var g = Gamepad.current;
				d = g.dpad.up.isPressed ? Vector3.up        // Dpad
					: g.dpad.right.isPressed ? Vector3.right
					: g.dpad.down.isPressed ? Vector3.down
					: g.dpad.left.isPressed ? Vector3.left
					: g.leftStick.up.isPressed ? Vector3.up     // Left Joystick
					: g.leftStick.right.isPressed ? Vector3.right
					: g.leftStick.down.isPressed ? Vector3.down
					: g.leftStick.left.isPressed ? Vector3.left
					: d;

				// SELECT = QUIT
				if (g.selectButton.wasPressedThisFrame) Application.Quit();
			}

			if (d != dpad)
			{
				dpad = d;
				foreach (var listener in listeners) listener.dpad = dpad;
				dpadChanged?.Invoke(dpad);
			}
		}
		#endregion
	}
}