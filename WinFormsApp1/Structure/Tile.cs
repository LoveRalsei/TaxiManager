using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaxiManager.Structure
{
    /// <summary>
    /// 地图上的一片区域的索引
    /// 其内部储存了一个32bit数，最高8bit代表其大小，中12bit代表X，最低12bit代表Y
    /// 其中大小以百米为一单位，即支持长宽 100m - 25.5km
    /// 索引则是以Position.MinX, Position.MinY处为(0, 0)
    /// </summary>
    public record struct Tile
    {
        private uint _data = 0;
        public byte Size {
            readonly get => (byte)(_data >> 24);
            set => _data = _data & 0x00ffffff | (uint) value << 24;
        }
        public readonly uint SizeMeter => (uint)Size * 100;
        public uint X
        {
            readonly get => (ushort)(_data >> 12 & 0xFFF);
            set => _data = _data & 0xff000fff | (value & 0xfff) << 12;
        }
        public uint Y
        {
            readonly get => (ushort)(_data & 0xFFF);
            set => _data = _data & 0xfffff000 | value & 0xfff;
        }
        public readonly PositionRange Range
        {
            get
            {
                uint length = SizeMeter, x = X * length + Position.MinX, y = Y * length + Position.MinY;
                return PositionRange.From(
                    Position.From(x, y),
                    Position.From(x + length, y + length)
                );
            }
        }
        public readonly List<Tile> SubTiles
        {
            get
            {
                var size = Size;
                if (size == 0)
                    throw new ArgumentException("The size of Tile can't be zero!");
                if (size == 1)
                    return [this];
                var list = new List<Tile>();
                uint x = X * size, y = Y * size;
                for (uint i=0; i<size; i++)
                {
                    for (uint j=0; j<size; j++)
                    {
                        list.Add(From(x+i, y+j));
                    }
                }
                return list;
            }
        }
        public Tile(byte size, uint x, uint y)
        {
            if (size == 0)
                throw new ArgumentException("The size of Tile can't be zero!");
            Size = size;
            X = x;
            Y = y;
        }
        /// <summary>
        /// 获得一个起始点相同，大小不同的瓦片
        /// </summary>
        public Tile ToSize(byte size)
        {
            var thisSize = Size;
            uint x = X * thisSize / size, y = Y * thisSize / size;
            return From(size, x, y);
        }
        public static Tile From(byte size, uint x, uint y) => new(size, x, y);
        public static Tile From(uint x, uint y) => From(1, x, y);
        
        public static Tile From(byte size, Position position)
        {
            uint length = (uint)(size * 100);
            if (length == 0) throw new ArgumentException("The size of Tile can't be zero!");
            return From(size, (position.X - Position.MinX) / length, (position.Y - Position.MinY) / length);
        }

        public static List<Tile> GetTilesIn(byte tileSize, PositionRange range)
        {
            var valid = range.ToValid();
            List<Tile> tiles = [];
            var min = valid.Min;
            var max = valid.Max;
            Tile tileA = min.GetTile(tileSize), tileB = max.GetTile(tileSize);
            uint xA = tileA.X, yA = tileA.Y, xB = tileB.X, yB = tileB.Y;
            // 当最大点刚好在瓦片边缘时
            // tileB会额外包含一个长度的瓦片群
            // 需要考虑边缘情况，进行剔除
            var rangeB = tileB.Range;
            if (max.X == rangeB.Min.X)
                xB--;
            if (max.Y == rangeB.Min.Y)
                yB--;
            for (uint i = xA; i <= xB; i++) for (uint j = yA; j <= yB; j++)
                tiles.Add(Tile.From(tileSize, i, j));
            return tiles;
        }

        public static List<Tile> GetAllTiles(byte tileSize = 1)
        {
            var range = PositionRange.From(Position.Min, Position.Max);
            return GetTilesIn(tileSize, range);
        }
    }
}
