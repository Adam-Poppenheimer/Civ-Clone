using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.Cities.Territory {

    /// <summary>
    /// The standard implementation of ICellPossessionCanon.
    /// </summary>
    public class CellPossessionCanon : ICellPossessionCanon {

        #region instance fields and properties

        private DictionaryOfLists<ICity, IHexCell> TilesOfCity = new DictionaryOfLists<ICity, IHexCell>();

        private Dictionary<IHexCell, ICity> CityOfTile = new Dictionary<IHexCell, ICity>();

        #endregion

        #region instance methods

        #region from ITilePossessionCanon

        /// <inheritdoc/>
        public bool CanChangeOwnerOfTile(IHexCell cell, ICity newOwner) {
            if(cell == null) {
                throw new ArgumentNullException("cell");
            }
            var currentOwner = GetCityOfTile(cell);
            if(currentOwner != null && currentOwner.Location == cell) {
                return false;
            }else {
                return GetCityOfTile(cell) != newOwner;
            }            
        }

        /// <inheritdoc/>
        public void ChangeOwnerOfTile(IHexCell cell, ICity newOwner) {
            if(cell == null) {
                throw new ArgumentNullException("cell");
            }else if(!CanChangeOwnerOfTile(cell, newOwner)) {
                throw new InvalidOperationException("CanChangeOwnerOfTile must be true on the given arguments");
            }

            var oldOwner = GetCityOfTile(cell);
            if(oldOwner != null) {
                TilesOfCity[oldOwner].Remove(cell);
            }

            CityOfTile[cell] = newOwner;

            if(newOwner != null) {
                TilesOfCity[newOwner].Add(cell);
            }
        }

        /// <inheritdoc/>
        public ICity GetCityOfTile(IHexCell cell) {
            if(cell == null) {
                throw new ArgumentNullException("cell");
            }

            ICity retval;
            CityOfTile.TryGetValue(cell, out retval);
            return retval;
        }

        /// <inheritdoc/>
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
