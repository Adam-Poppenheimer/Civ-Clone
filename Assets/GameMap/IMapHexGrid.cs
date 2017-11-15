using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityCustomUtilities.Grids;

namespace Assets.GameMap {

    public interface IMapHexGrid {

        #region properties

        ReadOnlyCollection<IMapTile> Tiles { get; }

        #endregion

        #region methods

        bool     HasTileOfCoords(HexCoords coords);
        IMapTile GetTileOfCoords(HexCoords coords);

        bool     HasNeighborInDirection(IMapTile center, int neighborDirection);
        IMapTile GetNeighborInDirection(IMapTile center, int neighborDirection);

        int GetDistance(IMapTile tileOne, IMapTile tileTwo);

        List<IMapTile> GetTilesInRadius(IMapTile center, int radius);

        List<IMapTile> GetTilesInLine(IMapTile start, IMapTile end);

        List<IMapTile> GetTilesInRing(IMapTile center, int radius);

        List<IMapTile> GetShortestPathBetween(IMapTile start, IMapTile end);

        List<IMapTile> GetShortestPathBetween(IMapTile start, IMapTile end, Func<IMapTile, int> costFunction);

        List<IMapTile> GetNeighbors(IMapTile center);

        void RegenerateMap();

        #endregion

    }

}
