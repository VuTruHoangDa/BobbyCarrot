using BobbyCarrot.Movers;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Tilemaps;
using UnityEngine.U2D;


namespace BobbyCarrot.Platforms
{
	public abstract class Platform : TileBase, IPlatform
	{
		/// <summary>
		/// Tạo instance mới > copy dữ liệu > khởi tạo dựa theo môi trường hiện tại
		/// </summary>
		protected virtual Platform Create()
		{
			var p = Instantiate(this);
			p.sprite = sprite;
			p.position = position;
			p.animationData = animationData;
			return p;
		}


		public static ushort id { get; private set; }

		private static SpriteAtlas atlas;

		protected static Transform anchor { get; private set; }

		private Sprite Δsprite;
		protected Sprite sprite
		{
			get => Δsprite;

			set
			{
				Δsprite = value;
				Refresh();
			}
		}

		public static readonly int Task_Platform_Init = "Platform.Init".GetHashCode();

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void Init()
		{
			atlas = Addressables.LoadAssetAsync<SpriteAtlas>("Assets/Platforms/Texture/Atlas.spriteatlasv2").WaitForCompletion();
			PlayGround.onAwake += () =>
			{
				array = Util.NewArray(Main.level.width, Main.level.height, (x, y) => new Stack<IPlatform>());
				maps = Addressables.InstantiateAsync("Assets/Platforms/Prefab/Maps.prefab")
					.WaitForCompletion().GetComponentsInChildren<Tilemap>();
				foreach (var map in maps)
				{
					map.origin = default;
					map.size = new(Main.level.width, Main.level.height);
				}

				anchor = maps[0].transform.parent;
			};

			PlayGround.onStart += async () =>
			{
				int count = 0;
				Vector3Int pos = default;
				PlayGround.taskList.Add(Task_Platform_Init);

				// Hiện animation "Loading x % ...."
				// Dùng count tính %

				for (pos.x = 0; pos.x < Main.level.width; ++pos.x)
					for (pos.y = 0; pos.y < Main.level.height; ++pos.y)
						foreach (var id in Main.level.platforms[pos.x][pos.y])
						{
							if ((++count) % 20 == 0) await UniTask.Yield();

							Platform.id = id;
							string name = "";
							if ((4 <= id && id <= 75 && id != 0 && id != 1
							&& id != 2 && id != 3 && id != 21 && id != 34
							&& id != 47 && id != 50 && id != 55 && id != 58)
							|| (id == 84) // Wind
							|| (89 <= id && id <= 95)
							|| (id == 109) // Rock
							|| (id == 119) || (id == 120) // Dragon head and body
							|| (id == 135) // Grass
							|| (id == 141) // Lock
							|| (240 <= id && id <= 244)
							|| (256 <= id && id <= 262)
							|| (269 <= id && id <= 335) // 269 = Snow
							|| (id == 374)) // Border
								name = "Assets/Platforms/Tiles/Obstacle.asset";
							else if ((0 <= id && id <= 3)
							|| (id == 21) || (id == 34) || (id == 47) || (id == 50)
							|| (id == 55) || (id == 58) // Door
							|| (id == 79)
							|| (id == 85) // Wind Stop
							|| (id == 121) // Dragon tail
							|| (176 <= id && id <= 182) // 180 = Ice, 181 = Start, 182 = Exit
							|| (id == 190)
							|| (192 <= id && id <= 239)
							|| (id == 245) || (id == 246) // Water
							|| (251 <= id && id <= 255) // 251, 252, 253 = Water
							|| (263 <= id && id <= 268)
							|| (359 <= id && id <= 361)
							|| (id == 368))
								name = "Assets/Platforms/Tiles/Ground.asset";
							else if (Item.itemIDs.Contains(id))
								name = "Assets/Platforms/Tiles/Item.asset";
							else if (80 <= id && id <= 82)
								name = "Assets/Platforms/Tiles/CloudGrid.asset";
							else if (96 <= id && id <= 98)
							{
								Push(pos, Addressables.InstantiateAsync("Assets/Movers/Prefab/Cloud.prefab", pos, Quaternion.identity)
									.WaitForCompletion().GetComponent<Cloud>());
								continue;
							}
							else if (id == 99)
								name = "Assets/Platforms/Tiles/Ice.asset";
							else if (id == 108)
							{
								Push(pos, Addressables.InstantiateAsync("Assets/Movers/Prefab/LotusLeaf.prefab", pos, Quaternion.identity)
									.WaitForCompletion().GetComponent<LotusLeaf>());
								continue;
							}
							else if (id == (ushort)Tree.Type.Root)
								name = "Assets/Platforms/Tiles/Tree.asset";
							else if (112 <= id && id <= 115)
								name = "Assets/Platforms/Tiles/Fan.asset";
							else if (id == 116)
								name = "Assets/Platforms/Tiles/Wood.asset";
							else if ((id == 128) || (id == 129) || (id == 130) || (id == 159))
								name = "Assets/Platforms/Tiles/BlockButton.asset";
							else if (131 <= id && id <= 134)
								name = "Assets/Platforms/Tiles/Block.asset";
							else if (136 <= id && id <= 138)
								name = "Assets/Platforms/Tiles/Carrot.asset";
							else if ((id == 139) || (id == 140))
								name = "Assets/Platforms/Tiles/Egg.asset";
							else if ((id == 144) || (id == 175))
								name = "Assets/Platforms/Tiles/Trap.asset";
							else if (145 <= id && id <= 148)
								name = "Assets/Platforms/Tiles/Mirror.asset";
							else if (149 <= id && id <= 152)
								name = "Assets/Platforms/Tiles/Conveyor.asset";
							else if (153 <= id && id <= 158)
								name = "Assets/Platforms/Tiles/Maze.asset";
							else if (id == 160 || id == 375)
								name = "Assets/Platforms/Tiles/TruckStation.asset";
							else if ((id == 161) || (id == 162))
								name = "Assets/Platforms/Tiles/ConveyorButton.asset";
							else if ((id == 163) || (id == 164))
								name = "Assets/Platforms/Tiles/MazeButton.asset";
							else if ((id == 165) || (id == 166))
								name = "Assets/Platforms/Tiles/WaterButton.asset";
							else if (167 <= id && id <= 174)
								name = "Assets/Platforms/Tiles/FanButton.asset";
							else if (247 <= id && id <= 250)
								name = "Assets/Platforms/Tiles/WaterFlow.asset";
							else continue;

							var tile = Addressables.LoadAssetAsync<Platform>(name).WaitForCompletion();
							tile.position = pos;
							tile.sprite = atlas.GetSprite(id.ToString());
							Push(tile.Create());
						}

				// Ẩn animation "Loading..."

				PlayGround.taskList.Remove(Task_Platform_Init);
			};
		}


		public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData) => tileData.sprite = sprite;


		#region Peek, Get, Push, Pop
		private static Stack<IPlatform>[][] array;
		private static Tilemap[] maps;
		public Vector3 position { get; protected set; }


		public static IPlatform Peek(Vector3 position) => array[(int)position.x][(int)position.y].Peek();


		/// <summary>
		/// Lưu ý: index==0 là trên cùng của stack, tương đương Peek()
		/// </summary>
		public static IPlatform Get(Vector3 position, int index) => array[(int)position.x][(int)position.y].ElementAt(index);


		public static void Push(Platform platform)
		{
			var stack = array[(int)platform.position.x][(int)platform.position.y];
			maps[stack.Count].SetTile(platform.position.ToVector3Int(), platform);
			stack.Push(platform);
		}


		public static void Push(Vector3 position, Mover mover)
		{
#if DEBUG
			if (mover is not IPlatform) throw new Exception($"{mover} phải là IPlatform mới có thể Push vô Platform !");
#endif
			mover.transform.parent = anchor;
			array[(int)position.x][(int)position.y].Push((IPlatform)mover);
		}


		public static void Push(IWayPoint wayPoint)
		{
#if DEBUG
			if (Peek(wayPoint.position) is IWayPoint) throw new Exception($"Chỉ duy nhất 1 WayPoint được phép ở trên cùng ! Waypoint= {wayPoint}");
#endif
			array[(int)wayPoint.position.x][(int)wayPoint.position.y].Push(wayPoint);
		}


		public static IPlatform Pop(Vector3 position)
		{
			var stack = array[(int)position.x][(int)position.y];
			var p = stack.Pop();
			if (p is Platform) maps[stack.Count].SetTile(position.ToVector3Int(), null);
			return p;
		}
		#endregion


		#region Animation
		private AnimationData ΔanimationData;
		public AnimationData animationData
		{
			get => ΔanimationData;

			set
			{
				ΔanimationData = value;
				Refresh();
			}
		}


		public override bool GetTileAnimationData(Vector3Int position, ITilemap tilemap, ref TileAnimationData outData)
		{
			if (animationData.animatedSprites == null || animationData.animatedSprites.Length == 0) return false;

			outData.animatedSprites = animationData.animatedSprites;
			outData.animationStartTime = animationData.animationStartTime;
			outData.animationSpeed = animationData.animationSpeed;
			outData.flags = animationData.flags;
			return true;
		}


		[Serializable]
		public struct AnimationData
		{
			public Sprite[] animatedSprites;
			public float animationSpeed, animationStartTime;
			public TileAnimationFlags flags;
		}
		#endregion


		private void Refresh()
		{
			var stack = array[(int)position.x][(int)position.y];
			int i = stack.Count - 1;
			foreach (var p in stack)
				if (p as Platform == this)
				{
					maps[i].RefreshTile(position.ToVector3Int());
					break;
				}
				else --i;
		}


		public virtual bool CanEnter(Mover mover) => true;
		public virtual void OnEnter(Mover mover) { }
		public virtual bool CanExit(Mover mover) => true;
		public virtual void OnExit(Mover mover) { }
	}



	public interface IPlatform
	{
		bool CanExit(Mover mover);

		void OnExit(Mover mover);

		bool CanEnter(Mover mover);

		void OnEnter(Mover mover);
	}
}