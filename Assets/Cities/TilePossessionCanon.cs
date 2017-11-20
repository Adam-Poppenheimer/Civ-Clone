using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using Assets.GameMap;

using UnityCustomUtilities.Extensions;

namespace Assets.Cities {

    public class TilePossessionCanon : ITilePossessionCanon {

        #region instance fields and properties

        private DictionaryOfLists<ICity, IMapTile> TilesOfCity = new DictionaryOfLists<ICity, IMapTile>();

        private Dictionary<IMapTile, ICity> CityOfTile = new Dictionary<IMapTile, ICity>();

        #endregion

        #region instance methods

        #region from ITilePossessionCanon

        public bool CanChangeOwnerOfTile(IMapTile tile, ICity newOwner) {
            if(tile == null) {
                throw new ArgumentNullException("tile");
            }

            return GetCityOfTile(tile) != newOwner;
        }

        public void ChangeOwnerOfTile(IMapTile tile, ICity newOwner) {
            if(tile == null) {
                throw new ArgumentNullException("tile");
            }else if(!CanChangeOwnerOfTile(tile, newOwner)) {
                throw new InvalidOperationException("CanChangeOwnerOfTile must be true on the given arguments");
            }

            var oldOwner = GetCityOfTile(tile);
            if(oldOwner != null) {
                TilesOfCity[oldOwner].Remove(tile);
            }

            CityOfTile[tile] = newOwner;

            if(newOwner != null) {
                TilesOfCity[newOwner].Add(tile);
            }
        }

        public ICity GetCityOfTile(IMapTile tile) {
            if(tile == null) {
                throw new ArgumentNullException("tile");
            }

            ICity retval;
            CityOfTile.TryGetValue(tile, out retval);
            return retval;
        }

        public IEnumerable<IMapTile> GetTilesOfCity(ICity city) {
            if(city == null) {
                throw new ArgumentNullException("city");
            }
            return TilesOfCity[city];
        }

        #endregion

        #endregion
        
    }

}
