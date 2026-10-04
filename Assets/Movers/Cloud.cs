using BobbyCarrot.Platforms;
using Cysharp.Threading.Tasks;
using RotaryHeart.Lib.SerializableDictionary;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace BobbyCarrot.Movers
{
	public sealed class Cloud : Mover, IPlatform
	{
		public Color color { get; private set; }
		[SerializeField] private SerializableDictionaryBase<Color, Sprite> sprites;
		private new void Awake()
		{
			base.Awake();
			color = Platform.id switch
			{
				96 => Color.Red,
				97 => Color.Violet,
				_ => Color.Green
			};
			spriteRenderer.sprite = sprites[color];
		}


		public void Move(Fan fan)
		{
#if DEBUG
			if (!fan.on) throw new Exception("Quạt đang tắt, không thể thổi mây !");
			if (isLock || direction != default) throw new Exception("Quạt không thể thổi mây ngay lập tức do mây đang bị khóa bởi CloudGrid hoặc mây đang di chuyển !");
#endif
			fans.Clear();
			fans.Add(fan);
			direction = fan.direction;
			Move().Forget();
		}


		public bool isLock { get; private set; }
		public void Lock()
		{
#if DEBUG
			if (isLock) throw new Exception("Mây đã bị khóa bởi CloudGrid, không thể khóa thêm lần nữa !");
#endif
			isLock = true;
			direction = default;
			fans.Clear();
		}


		private readonly List<Fan> fans = new();
		protected override async UniTask<bool> Move()
		{
			if (inertias.Contains(this)) inertias.Remove(this);
			else
			{
				if (balances.ContainsKey(this)) balances.Remove(this);
				if (woodBalances.ContainsKey(this)) woodBalances.Remove(this);
			}

			++count;
			if (!task.isRunning()) task = CheckBalancesAndInertias();
			while (true)
			{
				if (fans.Count == 2 && CanMove(fans[1].direction))
				{
					direction = fans[1].direction;
					fans.RemoveAt(0);
				}
				else if (!CanMove()) break;
				var t = base.Move();
				check = true;
				if (!await t) return false;
				if (isLock) return true;

				// Mây đỏ không bao giờ bị quạt khác đổi hướng
				if (color != Color.Red) FindNewFan();
			}

			--count;

			// Phòng trường hợp mây khác đỏ không thể di chuyển được bước nào
			if (color != Color.Red && fans.Count == 1) FindNewFan();
			Vector3 dest;   // Phải khai báo dest ở đây vì C# có bug về khai báo scope, đang report

			if (fans[0].on || fans.Count == 2)
			{
				// Tồn tại quạt đang thổi nhưng mây bị cản bởi Wood hoặc mây khác (không khóa) ?
				// Wood cản: mây có thể đi vào platform ngay dưới Wood ?
				// Hoặc mây khác cản: mây cản tương lai có thể không cản nữa ?
				// => Nếu vậy thì mây đang cân bằng động
				direction = default;
				foreach (var fan in fans)
				{
					if (!fan.on) continue;

					dest = transform.position + fan.direction;
					var platform = Platform.Peek(dest);
					if (platform is Wood wood && Platform.Get(dest, 1).CanEnter(this))
						(woodBalances.ContainsKey(this) ? woodBalances[this] : woodBalances[this] = new()).Add((fan, wood));
					else if (platform is Cloud destCloud && !destCloud.isLock)
					{
						if (destCloud.direction != default)
						{
							// Nếu mây cản sẽ không bị khóa ngay khi đến đích ?
							// Và mây cản không di chuyển ngược chiều mây hiện tại ?
							// Hoặc nếu ngược chiều thì tương lai mây cản có thể bị thổi lên
							// => Nếu vậy: thêm vào balances
							if (Platform.Get(dest, 1) is CloudGrid grid && grid.color == color) continue;

							if (destCloud.direction != -fan.direction)
							{
								(balances.ContainsKey(this) ? balances[this] : balances[this] = new()).Add(fan);
								if (!task.isRunning()) task = CheckBalancesAndInertias();
							}
							else foreach (var f in Fan.fans[Color.Yellow])
								if (Find(dest, f))
								{
									(balances.ContainsKey(this) ? balances[this] : balances[this] = new()).Add(fan);
									if (!task.isRunning()) task = CheckBalancesAndInertias();
									break;
								}

							continue;
						}

						// Mây cản đang đứng yên
						// Tạm thời chưa có cách nào kiểm tra xem mây cản có thể thoát khỏi dest ?
						(balances.ContainsKey(this) ? balances[this] : balances[this] = new()).Add(fan);
						if (!task.isRunning()) task = CheckBalancesAndInertias();
					}
				}

				return true;
			}

			// Không còn quạt nào đang thổi mây. Mây đang còn quán tính
			// Nếu vật cản là mây đang di chuyển và mây cản có thể thoát khỏi vị trí dest ?
			// => Nếu vậy: thêm vào inertias
			dest = transform.position + direction;

			// Phải đặt tên mới là dest_cloud và Grid vì C# có bug về khai báo scope, đang report
			if (Platform.Peek(dest) is not Cloud dest_cloud || dest_cloud.direction == default
				|| (Platform.Get(dest, 1) is CloudGrid Grid && Grid.color == dest_cloud.color))
			{
				direction = default;
				return true;
			}

			if (dest_cloud.direction != -direction)
			{
				inertias.Add(this);
				if (!task.isRunning()) task = CheckBalancesAndInertias();
				return true;
			}

			foreach (var fan in Fan.fans[Color.Yellow])
				if (Find(dest, fan))
				{
					inertias.Add(this);
					if (!task.isRunning()) task = CheckBalancesAndInertias();
					return true;
				}

			direction = default;
			return true;


			void FindNewFan()
			{
				// Kiểm tra có quạt nào thổi hướng vuông góc với hướng hiện tại không ?
				// Nếu có thì ưu tiên hướng mới cho bước di chuyển tiếp theo
				var pos = transform.position;
				if (fans.Count == 2) fans.RemoveAt(1);
				foreach (var fan in Fan.fans[direction == Vector3.up ? color : Color.Yellow])
					if (fan.on && Find(pos, fan))
					{
						fans.Add(fan);
						return;
					}
			}


			// Tìm quạt nằm ở vị trí thích hợp có thể thổi mây
			static bool Find(Vector3 cloud, Fan fan) => fan.color switch
			{
				Color.Green => cloud.x < fan.position.x && cloud.y == fan.position.y,
				Color.Red => cloud.x == fan.position.x && cloud.y < fan.position.y,
				Color.Violet => cloud.x > fan.position.x && cloud.y == fan.position.y,
				_ => cloud.x == fan.position.x && cloud.y > fan.position.y,
			};
		}


		public bool CanEnter(Mover mover) =>
			mover is Flyer or Fireball || (direction == default && mover is Bobby);


		public void OnEnter(Mover mover)
		{
			if (mover is not Bobby) return;

			mover.transform.parent = transform;
			mover.transform.localPosition = new(0, 0.4f);
			if (!wood) return;

			MoveWoodBalances(wood);
			wood = null;
		}


		public bool CanExit(Mover mover) => mover is not Bobby || direction == default;


		public void OnExit(Mover mover)
		{
			if (mover is not Bobby) return;

			mover.transform.parent = null;
			mover.transform.position = transform.position;
		}


		private static UniTask task;
		private static int count;
		private static bool check;
		private static readonly Dictionary<Cloud, List<Fan>> balances = new();
		private static readonly List<Cloud> inertias = new();
		private static readonly List<Cloud> tmpClouds = new();
		private static async UniTask CheckBalancesAndInertias()
		{
			var token = PlayGround.Token;
			while (true)
			{
				await UniTask.NextFrame();
				if (token.IsCancellationRequested || count == 0 || (balances.Count == 0 && inertias.Count == 0)) return;

				if (!check) continue;

				check = false;

				// Kiểm tra balances
				tmpClouds.Clear();
				foreach (var cloud_fans in balances)
					foreach (var fan in cloud_fans.Value)
						if (cloud_fans.Key.CanMove(fan.direction))
						{
							var cloud = cloud_fans.Key;
							cloud.fans.Clear();
							cloud.fans.Add(fan);
							cloud.direction = fan.direction;
							tmpClouds.Add(cloud);
							break;
						}

				foreach (var cloud in tmpClouds) cloud.Move().Forget();

				// Kiểm tra inertias
				tmpClouds.Clear();
				tmpClouds.AddRange(inertias);
				foreach (var cloud in tmpClouds) cloud.Move().Forget();
			}
		}


		private static readonly Dictionary<Cloud, List<(Fan, Wood)>> woodBalances = new();
		private static Wood wood;
		public static void OnWoodDeleted(Wood wood, Mover mover)
		{
			if (mover is Bobby && Platform.Peek(mover.transform.position + mover.direction) is Cloud cloud
				&& woodBalances.ContainsKey(cloud))
			{
				// Bobby đang đi đến mây trong hệ cân bằng động, sẽ xử lý khi Bobby tới mây 
				Cloud.wood = wood;
				return;
			}

			MoveWoodBalances(wood);
		}


		private static void MoveWoodBalances(Wood wood)
		{
			tmpClouds.Clear();
			foreach (var cloud_list in woodBalances)
				foreach (var (f, w) in cloud_list.Value)
					if (w == wood)
					{
						var cloud = cloud_list.Key;
						cloud.fans.Clear();
						cloud.fans.Add(f);
						cloud.direction = f.direction;
						tmpClouds.Add(cloud);
						break;
					}

			foreach (var cloud in tmpClouds.Random()) cloud.Move().Forget();
		}


		public static void OnTurnOffFans()
		{
			// Xóa quạt cùng color khỏi balances, xóa mây không còn quạt
			tmpClouds.Clear();
			foreach (var cloud_fans in balances)
				foreach (var fan in cloud_fans.Value)
					if (!fan.on)
					{
						cloud_fans.Value.Remove(fan);
						if (cloud_fans.Value.Count == 0) tmpClouds.Add(cloud_fans.Key);
						break;
					}

			foreach (var cloud in tmpClouds) balances.Remove(cloud);

			// Xóa quạt cùng color khỏi woodBalances, xóa mây không còn quạt
			tmpClouds.Clear();
			foreach (var cloud_list in woodBalances)
				foreach (var (fan, wood) in cloud_list.Value)
					if (!fan.on)
					{
						cloud_list.Value.Remove((fan, wood));
						if (cloud_list.Value.Count == 0) tmpClouds.Add(cloud_list.Key);
						break;
					}

			foreach (var cloud in tmpClouds) woodBalances.Remove(cloud);
		}


		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void Init() => PlayGround.onAwake += () =>
		{
			task = UniTask.CompletedTask;
			count = 0;
			check = false;
			balances.Clear();
			inertias.Clear();
			woodBalances.Clear();
			wood = null;
		};
	}
}