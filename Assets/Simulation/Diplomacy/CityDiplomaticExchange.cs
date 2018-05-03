using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Cities;
using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Diplomacy {

    public class CityDiplomaticExchange : DiplomaticExchangeBase {

        #region instance fields and properties

        #region from IDiplomaticExchange

        public override ExchangeType Type {
            get { return ExchangeType.City; }
        }

        public override bool RequiresIntegerInput {
            get { return false; }
        }

        #endregion

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

        public override bool CanExecuteBetweenCivs(ICivilization fromCiv, ICivilization toCiv) {
            if(CityInput == null) {
                return false;
            }

            var cityOwner = CityPossessionCanon.GetOwnerOfPossession(CityInput);

            return cityOwner == fromCiv && CityPossessionCanon.CanChangeOwnerOfPossession(CityInput, toCiv);
        }

        public override IOngoingDiplomaticExchange ExecuteBetweenCivs(ICivilization fromCiv, ICivilization toCiv) {
            if(!CanExecuteBetweenCivs(fromCiv, toCiv)) {
                throw new InvalidOperationException("CanExecuteBetweenCivs must return true on the given arguments");
            }

            CityPossessionCanon.ChangeOwnerOfPossession(CityInput, toCiv);

            return null;
        }

        public override string GetSummary() {
            return string.Format("{0} ({1})", CityInput.Name, CityInput.Population);
        }

        #endregion

        #endregion
        
    }

}
