using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Cities;
using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Diplomacy {

    public class CityDiplomaticExchange : IDiplomaticExchange {

        #region instance fields and properties

        #region from IDiplomaticExchange

        public bool RequiresIntegerInput {
            get { return false; }
        }

        public int IntegerInput { get; set; }

        #endregion

        public ICity CityToExchange { get; set; }

        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;

        #endregion

        #region constructors

        [Inject]
        public CityDiplomaticExchange(IPossessionRelationship<ICivilization, ICity> cityPossessionCanon) {
            CityPossessionCanon = cityPossessionCanon;
        }

        #endregion

        #region instance methods

        #region from IDiplomaticExchange

        public bool OverlapsWithExchange(IDiplomaticExchange exchange) {
            var cityExchange = exchange as CityDiplomaticExchange;
            return cityExchange != null && cityExchange.CityToExchange == CityToExchange;
        }

        public bool CanExecuteBetweenCivs(ICivilization fromCiv, ICivilization toCiv) {
            if(CityToExchange == null) {
                return false;
            }

            var cityOwner = CityPossessionCanon.GetOwnerOfPossession(CityToExchange);

            return cityOwner == fromCiv && CityPossessionCanon.CanChangeOwnerOfPossession(CityToExchange, toCiv);
        }

        public IOngoingDiplomaticExchange ExecuteBetweenCivs(ICivilization fromCiv, ICivilization toCiv) {
            if(!CanExecuteBetweenCivs(fromCiv, toCiv)) {
                throw new InvalidOperationException("CanExecuteBetweenCivs must return true on the given arguments");
            }

            CityPossessionCanon.ChangeOwnerOfPossession(CityToExchange, toCiv);

            return null;
        }

        public string GetSummary() {
            return string.Format("{0} ({1})", CityToExchange.gameObject.name, CityToExchange.Population);
        }

        #endregion

        #endregion
        
    }

}
