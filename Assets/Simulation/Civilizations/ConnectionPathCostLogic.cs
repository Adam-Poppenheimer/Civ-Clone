using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Buildings;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.Civilizations {

    public class ConnectionPathCostLogic : IConnectionPathCostLogic {

        #region instance fields and properties
        
        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;
        private IPossessionRelationship<IHexCell, ICity>      CityLocationCanon;
        private IPossessionRelationship<ICity, IBuilding>     BuildingPossessionCanon;

        #endregion

        #region constructors

        [Inject]
        public ConnectionPathCostLogic(
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon,
            IPossessionRelationship<IHexCell, ICity>      cityLocationCanon,
            IPossessionRelationship<ICity, IBuilding>     buildingPossessionCanon
        ) {
            CityPossessionCanon     = cityPossessionCanon;
            CityLocationCanon       = cityLocationCanon;
            BuildingPossessionCanon = buildingPossessionCanon;
        }

        #endregion

        #region instance methods

        #region from IConnectionPathCostLogic

        public Func<IHexCell, IHexCell, float> BuildPathCostFunction(ICivilization civOne, ICivilization civTwo) {
            return delegate(IHexCell fromCell, IHexCell toCell) {
                return IsMovementValidForConnection(fromCell, toCell, civOne, civTwo) ? 1 : -1;
            };
        }

        #endregion

        private bool IsMovementValidForConnection(IHexCell fromCell, IHexCell toCell, ICivilization civOne, ICivilization civTwo) {
            if(toCell.IsUnderwater) {

                if(fromCell.IsUnderwater) {
                    return true;
                }

                var cityAtFromCell = CityLocationCanon.GetPossessionsOfOwner(fromCell).FirstOrDefault();
                if(cityAtFromCell == null) {
                    return false;
                }

                var cityOwner = CityPossessionCanon.GetOwnerOfPossession(cityAtFromCell);
                if(cityOwner != civOne && cityOwner != civTwo) {
                    return false;
                }

                return IsCityValidHarbor(cityAtFromCell);

            }else if(toCell.HasRoads){

                return true;

            }else {
                var cityAtToCell = CityLocationCanon.GetPossessionsOfOwner(toCell).FirstOrDefault();
                if(cityAtToCell == null) {
                    return false;
                }

                var cityOwner = CityPossessionCanon.GetOwnerOfPossession(cityAtToCell);
                if(cityOwner != civOne && cityOwner != civTwo) {
                    return false;
                }

                return !fromCell.IsUnderwater || toCell.IsUnderwater || IsCityValidHarbor(cityAtToCell);
            }
        }

        private bool IsCityValidHarbor(ICity city) {
            return BuildingPossessionCanon.GetPossessionsOfOwner(city).Exists(
                building => building.Template.ProvidesOverseaConnection
            );
        }

        #endregion

    }

}
