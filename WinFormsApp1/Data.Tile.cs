using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaxiManager
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
            init => _data = (_data & 0x00ffffff) | (((uint) value) << 24);
        }
        public readonly uint SizeMeter => ((uint)Size) * 100;
        public uint X
        {
            readonly get => (ushort)((_data >> 12) & 0xFFF);
            init => _data = (_data & 0xff000fff) | ((value & 0xfff) << 12);
        }
        public uint Y
        {
            readonly get => (ushort)(_data & 0xFFF);
            init => _data = (_data & 0xfffff000) | (value & 0xfff);
        }
        public readonly PositionRange Range
        {
            get
            {
                uint length = SizeMeter, x = X * length + Position.MinX, y = Y * length + Position.MinY;
                return PositionRange.Make(
                    Position.Make(x, y),
                    Position.Make(x + length, y + length)
                );
            }
        }
        private Tile(byte size, uint x, uint y)
        {
            Size = size;
            X = x;
            Y = y;
        }
        public static Tile Make(byte size, uint x, uint y)
        {
            return new(size, x, y);
        }
        public static Tile Make(uint x, uint y)
        {
            return Make(1, x, y);
        }
        public static Tile Make(byte size, Position position)
        {
            uint length = (uint)(size * 100);
            if (length == 0) throw new ArgumentException("The size of Tile can't be zero!");
            return Tile.Make(size, (position.X - Position.MinX) / length, (position.Y - Position.MinY) / length);
        }
    }
}
