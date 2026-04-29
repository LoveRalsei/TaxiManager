using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaxiManager
{
    public class ServiceF3 : IServiceF3
    {
        public static readonly ServiceF3 Instance = new();
        uint IServiceF3.CountDrivers(PositionRange range, DateTime from, DateTime to)
        {
            if (from > to)
                throw new ArgumentException("The arg from should not be greater than the arg to.");
            uint count = 0;
            HashSet<Tile> rangeTiles = [.. range.GetTiles(Driver.ExistTileSize)];
            foreach (var driver in DataLoader.Drivers)
            {
                if (!driver.IsExist(from, to) || !driver.IsExist(rangeTiles))
                    continue;
                var fromIndex = driver.GetPositionIndex(from);
                var toIndex = driver.GetPositionIndex(to);
                if (fromIndex == null || toIndex == null)
                    continue;
                // 如果插值左节点相同，说明其在同一个插值范围，只需要判定左右两侧节点是否在范围中
                if (fromIndex.Value.indexLeft == toIndex.Value.indexLeft)
                {
                    if (range.IsIn(driver.Nodes[(int)fromIndex.Value.indexLeft].Position) ||
                        range.IsIn(driver.Nodes[(int)fromIndex.Value.indexRight].Position))
                        count++;
                    continue;
                }
                // 左侧插值节点取左，右侧插值节点取右，这样可以获取到的结果判定相对宽松
                uint left = fromIndex.Value.indexLeft;
                uint right = toIndex.Value.indexRight;
                for (int i = (int)left; i <= right; i++)
                {
                    if (range.IsIn(driver.Nodes[i].Position))
                    {
                        count++;
                        break;
                    }
                }
            }
            return count;
        }
    }
}
