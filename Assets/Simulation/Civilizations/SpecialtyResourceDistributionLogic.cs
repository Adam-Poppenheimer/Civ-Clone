using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.SpecialtyResources;
using Assets.Simulation.Cities;
using Assets.Simulation.HexMap;

namespace Assets.Simulation.Civilizations {

    public class SpecialtyResourceDistributionLogic : ISpecialtyResourceDistributionLogic {

        #region instance fields and properties

        private ISpecialtyResourcePossessionLogic ResourcePossessionCanon;

        private IResourceAssignmentCanon AssignmentCanon;

        private IPossessionRelationship<IHexCell, IResourceNode> NodePositionCanon;

        private IHealthLogic HealthLogic;

        private IHappinessLogic HappinessLogic;

        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;

        #endregion

        #region constructors

        public SpecialtyResourceDistributionLogic(
            ISpecialtyResourcePossessionLogic resourcePossessionCanon,
            IResourceAssignmentCanon assignmentCanon,
            IPossessionRelationship<IHexCell, IResourceNode> nodePositionCanon,
            IHealthLogic healthLogic, IHappinessLogic happinessLogic,
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon
        ) {
            ResourcePossessionCanon = resourcePossessionCanon;
            AssignmentCanon         = assignmentCanon;
            NodePositionCanon       = nodePositionCanon;
            HealthLogic             = healthLogic;
            HappinessLogic          = happinessLogic;
            CityPossessionCanon     = cityPossessionCanon;
        }

        #endregion

        #region instance methods

        #region from ISpecialtyResourceDistributionLogic

        public void DistributeResourcesOfCiv(ICivilization civ) {
            var citiesOfCiv = CityPossessionCanon.GetPossessionsOfOwner(civ);

            ClearAssignments(citiesOfCiv);
            AssignResourcesLocally(civ, citiesOfCiv);
            AssignResourcesWhereNeeded(civ, citiesOfCiv);
        }

        #endregion

        private void ClearAssignments(IEnumerable<ICity> cities) {
            foreach(var city in cities) {
                AssignmentCanon.UnassignAllResourcesFromCity(city);
            }
        }

        private void AssignResourcesLocally(ICivilization civ, IEnumerable<ICity> cities) {
            foreach(var city in cities) {
                foreach(var nodes in GetNodesLocalToCity(city)) {
                    if(AssignmentCanon.CanAssignResourceToCity(nodes.Resource, city)) {
                        AssignmentCanon.AssignResourceToCity(nodes.Resource, city);
                    }
                }
            }
        }

        private IEnumerable<IResourceNode> GetNodesLocalToCity(ICity city) {
            return NodePositionCanon.GetPossessionsOfOwner(city.Location);
        }

        private void AssignResourcesWhereNeeded(ICivilization civ, IEnumerable<ICity> cities) {
            AssignNeededBonusResources(civ, cities);
            AssignNeededLuxuryResources(civ, cities);
        }

        private void AssignNeededBonusResources(ICivilization civ, IEnumerable<ICity> cities) {
            var cityQueue = new Queue<ICity>(cities);

            var availableResources = GetAvailableResources(civ, SpecialtyResourceType.Bonus);

            foreach(var currentResource in availableResources) {
                while(cityQueue.Count > 0 && AssignmentCanon.GetFreeCopiesOfResourceForCiv(currentResource, civ) > 0) {
                    var currentCity = cityQueue.Dequeue();
                    var cityHealth = HealthLogic.GetHealthOfCity(currentCity);

                    if(cityHealth < 0) {
                        if(AssignmentCanon.CanAssignResourceToCity(currentResource, currentCity)) {
                            AssignmentCanon.AssignResourceToCity(currentResource, currentCity);
                        }
                        if(cityHealth < -1) {
                            cityQueue.Enqueue(currentCity);
                        }
                    }
                }
            }            
        }

        private void AssignNeededLuxuryResources(ICivilization civ, IEnumerable<ICity> cities) {
            var cityQueue = new Queue<ICity>(cities);

            var availableResources = GetAvailableResources(civ, SpecialtyResourceType.Luxury);
            var nextResource = availableResources.FirstOrDefault();

            while(cityQueue.Count > 0 && nextResource != null) {
                var currentCity = cityQueue.Dequeue();
                var cityHappiness = HappinessLogic.GetHappinessOfCity(currentCity);

                if(cityHappiness < 0) {
                    if(AssignmentCanon.CanAssignResourceToCity(nextResource, currentCity)) {
                        AssignmentCanon.AssignResourceToCity(nextResource, currentCity);
                    }
                    if(cityHappiness < -1) {
                        cityQueue.Enqueue(currentCity);
                    }
                }
            }
        }

        private IEnumerable<ISpecialtyResourceDefinition> GetAvailableResources(ICivilization civ, SpecialtyResourceType type) {
            return ResourcePossessionCanon
                .GetFullResourceSummaryForCiv(civ)
                .Where(keyvalue => keyvalue.Key.Type == type && keyvalue.Value > 0)
                .Select(keyvalue => keyvalue.Key);
        }

        #endregion

    }

}
