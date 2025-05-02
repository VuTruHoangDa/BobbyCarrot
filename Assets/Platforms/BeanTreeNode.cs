using BobbyCarrot.Movers;
using Cysharp.Threading.Tasks;
using RotaryHeart.Lib.SerializableDictionary;
using UnityEngine;
using UnityEngine.AddressableAssets;


namespace BobbyCarrot.Platforms
{
	[CreateAssetMenu(fileName = "BeanTreeNode", menuName = "Platforms/BeanTreeNode")]
	public sealed class BeanTreeNode : Platform
	{
		private enum Type : ushort
		{
			Seed = 127, Little = 111, Root = 110, Body = 126, Top = 142
		}
		private Type type;

		[SerializeField] private SerializableDictionaryBase<Type, Sprite> sprites;

		private int height;

		protected override Platform Create()
		{
			if (id != (ushort)Type.Root) return null;

			var p = base.Create() as BeanTreeNode;
			p.sprites = sprites;
			p.delay = delay;
			p.sprite = sprites[p.type = Type.Seed];
			p.height = 1;

			var index = p.index;
			do
			{
				++p.height;
				index += Vector3Int.up;
			} while (!IsTop(Main.level.platforms[index.x][index.y]));
			return p;


			static bool IsTop(in ReadOnlyArray<ushort> ids)
			{
				for (int i = ids.Length - 1; i >= 0; --i)
				{
					if (ids[i] == (ushort)Type.Top) return true;
					if (ids[i] == (ushort)Type.Body) break;
				}

				return false;
			}
		}


		public override bool CanEnter(Mover mover) => mover is Flyer or Fireball or Bobby;


		[SerializeField] private int delay;
		public override async void OnEnter(Mover mover)
		{
			if (mover is not Bobby || type != Type.Seed) return;

			if (PlayGround.items[Item.Type.Bean] == 0)
			{
				// Hiện UI yêu cầu hạt đậu
				return;
			}

			--PlayGround.items[Item.Type.Bean];
			sprite = sprites[type = Type.Little];
			var token = PlayGround.Token;
			await UniTask.Delay(delay);
			if (token.IsCancellationRequested) return;

			sprite = sprites[type = Type.Root];
			var asset = Addressables.LoadAssetAsync<BeanTreeNode>("Assets/Platforms/Tiles/BeanTreeNode.asset")
				.WaitForCompletion();
			var index = this.index;
			BeanTreeNode node;
			for (int i = height - 2; i > 0; --i)
			{
				node = Instantiate(asset);
				node.sprite = sprites[node.type = Type.Body];
				Push(index += Vector3Int.up, node);
				await UniTask.Delay(delay);
				if (token.IsCancellationRequested) return;
			}

			node = Instantiate(asset);
			node.sprite = sprites[node.type = Type.Top];
			Push(index += Vector3Int.up, node);
		}
	}
}