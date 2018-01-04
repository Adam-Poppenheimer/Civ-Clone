using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.Cities.Territory {

    public class TilePossessionCanon : ITilePossessionCanon {

        #region instance fields and properties

        private DictionaryOfLists<ICity, IHexCell> TilesOfCity = new DictionaryOfLists<ICity, IHexCell>();

        private Dictionary<IHexCell, ICity> CityOfTile = new Dictionary<IHexCell, ICity>();

        #endregion

        #region instance methods

        #region from ITilePossessionCanon

        public bool CanChangeOwnerOfTile(IHexCell tile, ICity newOwner) {
            if(tile == null) {
                throw new ArgumentNullException("tile");
            }
            var currentOwner = GetCityOfTile(tile);
            if(currentOwner != null && currentOwner.Location == tile) {
                return false;
            }else {
                return GetCityOfTile(tile) != newOwner;
            }            
        }

        public void ChangeOwnerOfTile(IHexCell tile, ICity newOwner) {
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

        public ICity GetCityOfTile(IHexCell tile) {
            if(tile == null) {
                throw new ArgumentNullException("tile");
            }

            ICity retval;
            CityOfTile.TryGetValue(tile, out retval);
            return retval;
        }

        public IEnumerable<IHexCell> GetTilesOfCity(ICity city) {
            if(city == null) {
                throw new ArgumentNullException("city");
            }
            return TilesOfCity[city];
        }

        #endregion

        #endregion
        
    }

}
