using BobbyCarrot.Movers;
using BobbyCarrot.Platforms;
using System;
using UnityEngine;


namespace BobbyCarrot
{
	[RequireComponent(typeof(UnityEngine.Camera))]
	public class Camera : MonoBehaviour
	{
		private static Camera instance;
		[SerializeField] private UnityEngine.Camera cam;
		private Rect mapRect;
		private void Awake()
		{
#if DEBUG
			if (enabled) throw new Exception("Phải tắt script Camera, gameObject bật !");
#endif
			instance = instance ? throw new Exception("Chỉ được 1 Camera trong Scene !") : this;
			cam.aspect = Screen.width / (float)Screen.height;
			mapRect = new(0.5f, 0.5f, Main.map.width - 2, Main.map.height - 2);
			transform.position = new(mapRect.center.x, mapRect.center.y, -10);
		}


		private static Transform mover;
		private static Vector3 lastPos;
		public static void Focus(Mover mover)
		{
#if DEBUG
			if (!mover || !mover.gameObject.activeSelf)
				throw new InvalidOperationException("Mover không tồn tại hoặc đang đắt !");
			if (mover is IPlatform)
				throw new InvalidOperationException("Không thể focus IPlatform (lá sen, mây...) !");
#endif
			Camera.mover = mover.transform;
			lastPos = default;
			instance.enabled = true;
		}


		private void Update()
		{
			if (!mover.gameObject.activeSelf)
			{
				enabled = false;
				return;
			}

			if (mover.position == lastPos) return;

			var rect = new Rect()
			{
				width = cam.orthographicSize * 2 * cam.aspect,
				height = cam.orthographicSize * 2,
				center = lastPos = mover.position
			};
			rect.x = Mathf.Clamp(rect.x, mapRect.x, mapRect.xMax - rect.width);
			rect.y = Mathf.Clamp(rect.y, mapRect.y, mapRect.yMax - rect.height);
			transform.position = new(rect.center.x, rect.center.y, -10);
		}
	}
}