using BobbyCarrot.Movers;
using Cysharp.Threading.Tasks;
using RotaryHeart.Lib.SerializableDictionary;
using UnityEngine;


namespace BobbyCarrot.Platforms
{
	[CreateAssetMenu(fileName = "Tree", menuName = "Platforms/Tree")]
	public sealed class Tree : Platform
	{
		public enum Type : ushort
		{
			Garden = 127, Little = 111, Root = 110, Body = 126, Top = 142
		}
		private Type type;


		protected override Platform Create()
		{
			var p = base.Create() as Tree;
			p.sprites = sprites;
			p.delay = delay;
			p.sprite = sprites[p.type = Type.Garden];
			p.height = 1;

			// Quét Main.map tìm chiều cao cây
			var pos = p.position.ToVector3Int();
			while (true)
			{
				++p.height;
				pos += Vector3Int.up;
				var ids = Main.map.platforms[pos.x][pos.y];
				for (int i = ids.Count - 1; i >= 0; --i)
				{
					if (ids[i] == (ushort)Type.Top) return p;
					if (ids[i] == (ushort)Type.Body) break;
				}
			}
		}


		public override bool CanEnter(Mover mover) => mover is Flyer or Fireball or Bobby;


		[SerializeField] private SerializableDictionaryBase<Type, Sprite> sprites;
		private int height;
		[SerializeField] private int delay;
		public override async void OnEnter(Mover mover)
		{
			if (mover is not Bobby || type != Type.Garden) return;

			if (PlayGround.items[Item.Type.Seed] == 0)
			{
				// Hiện UI yêu cầu hạt đậu
				return;
			}

			--PlayGround.items[Item.Type.Seed];
			sprite = sprites[type = Type.Little];
			var token = PlayGround.Token;
			await UniTask.Delay(delay);
			if (token.IsCancellationRequested) return;

			sprite = sprites[type = Type.Root];
			var pos = position;
			Tree node;
			for (int i = height - 2; i > 0; --i)
			{
				node = Instantiate(this);
				node.sprite = sprites[node.type = Type.Body];
				node.position = pos += Vector3.up;
				Push(node);
				await UniTask.Delay(delay);
				if (token.IsCancellationRequested) return;
			}

			node = Instantiate(this);
			node.sprite = sprites[node.type = Type.Top];
			node.position = pos += Vector3.up;
			Push(node);
		}
	}
}