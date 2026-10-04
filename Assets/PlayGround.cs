using BobbyCarrot.MapEditors;
using BobbyCarrot.Movers;
using BobbyCarrot.Platforms;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;


namespace BobbyCarrot
{
	public sealed class PlayGround : MonoBehaviour
	{
		public static event Action onAwake;

		/// <summary>
		/// Bắt đầu task thì taskList.Add(TASK_ID)<br/>
		/// Kết thúc task thì taskList.Remove(TASK_ID)<para/>
		/// Đảm bảo PlayGround sẽ không End() cho đến khi tất cả task và tất cả Start() (của obj mới sinh) chạy xong hết
		/// </summary>
		public static Action onStart;

		/// <summary>
		/// Chỉ thêm hoặc xóa TASK_ID khi onStart đang chạy
		/// </summary>
		public static readonly List<int> taskList = new();

		public MapEditor editor; // Test

		public static readonly Dictionary<Item.Type, ushort> items = new();

		public static int egg, carrot;


		private void Awake()
		{
			// Đảm bảo UI (nút Thoát, nút Chơi lại...) đang tắt
			Main.level = new(editor.CreateMapFile());
			Destroy(editor.gameObject);
			Camera.main.aspect = Main.level.width / (float)Main.level.height;
			Camera.main.transform.position = new(Main.level.width / 2f - 0.5f, Main.level.height / 2f - 0.5f, -10);
			Camera.main.orthographicSize = Main.level.height / 2f - 1;

			cts?.Dispose();
			cts = new();
			taskList.Clear();
			foreach (Item.Type key in Enum.GetValues(typeof(Item.Type))) items[key] = 0;

			// Test
			items[Item.Type.Shovel] = 1;
			items[Item.Type.Seed] = 10;
			items[Item.Type.Gas] = 1;

			onAwake();
		}


		private async void Start()
		{
			onStart();

			do await UniTask.Yield();
			while (taskList.Count != 0); // Đợi tất cả task chạy xong
			await UniTask.NextFrame();  // Đợi tất cả Start() chạy xong (nếu có obj mới tạo)

			Mover.Show<Bobby>(Ground.startPoint, Vector3.down);

			// Bật UI lên (nút Thoát, nút Chơi lại...)
		}


		private static CancellationTokenSource cts;
		public static CancellationToken Token => cts.Token;
		public static void End()
		{
			var bobby = Mover.Get<Bobby>();
			if (bobby) bobby.gameObject.SetActive(false);
			var truck = Mover.Get<Truck>();
			if (truck) truck.gameObject.SetActive(false);

			cts.Cancel();

		}


		// Fix tạm thời: thoát Play mode thì hủy task
#if UNITY_EDITOR
		private void OnDisable()
		{
			cts.Cancel();
		}
#endif
	}
}