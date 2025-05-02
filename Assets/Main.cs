using BobbyCarrot.Movers;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


namespace BobbyCarrot
{
	public sealed class Main : MonoBehaviour
	{
		private Resolution fullRes;
		private void Awake()
		{
			foreach (var r in Screen.resolutions)
				if (r.width > fullRes.width || r.height > fullRes.height) fullRes = r;

			Screen.SetResolution(fullRes.width, fullRes.height, true);
		}


		public static Level level; /*{ get; private set; }*/
		public static void NextLevel()
		{

		}


		public static Vector3 dpad { get; private set; }
		public static event Action<Vector3> dpadChanged;
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

				// Move
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
			}

			if (d != dpad)
			{
				dpad = d;
				foreach (var listener in listeners) listener.dpad = dpad;
				dpadChanged?.Invoke(dpad);
			}
		}


		private static readonly List<IGamepadListener> listeners = new();
		public static void AddListener(IGamepadListener listener)
		{
			if (!listeners.Contains(listener))
			{
				listeners.Add(listener);
				listener.dpad = dpad;
			}
		}


		public static void RemoveListener(IGamepadListener listener)
		{
			if (listeners.Contains(listener))
			{
				listeners.Remove(listener);
				listener.dpad = default;
			}
		}
	}
}