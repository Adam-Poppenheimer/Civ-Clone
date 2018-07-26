using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using UniRx;

using Assets.Simulation.Cities;
using Assets.Simulation.Improvements;
using Assets.Simulation.MapResources;
using Assets.Simulation.HexMap;

namespace Assets.Simulation.Civilizations {

    public class ResourceTransferCanonSynchronizer {

        #region instance fields and properties

        private IResourceTransferCanon                           ResourceTransferCanon;
        private IImprovementLocationCanon                        ImprovementLocationCanon;
        private IPossessionRelationship<IHexCell, IResourceNode> ResourceNodeLocationCanon;
        private ICivilizationTerritoryLogic                      CivTerritoryLogic;
        private IPossessionRelationship<ICity, IHexCell>         CityTerritoryCanon;
        private IPossessionRelationship<ICivilization, ICity>    CityPossessionCanon;

        #endregion

        #region constructors

        public ResourceTransferCanonSynchronizer(
            IResourceTransferCanon                           resourceTransferCanon,
            IImprovementLocationCanon                        improvementLocationCanon,
            IPossessionRelationship<IHexCell, IResourceNode> resourceNodeLocationCanon,
            ICivilizationTerritoryLogic                      civTerritoryLogic,
            IPossessionRelationship<ICity, IHexCell>         cityTerritoryCanon,
            IPossessionRelationship<ICivilization, ICity>    cityPossessionCanon,
            ImprovementSignals                               improvementSignals,
            ResourceSignals                         resourceSignals,
            CitySignals                                      citySignals,
            CivilizationSignals                              civSignals
        ) {
            ResourceTransferCanon     = resourceTransferCanon;
            ImprovementLocationCanon  = improvementLocationCanon;
            ResourceNodeLocationCanon = resourceNodeLocationCanon;
            CivTerritoryLogic         = civTerritoryLogic;
            CityTerritoryCanon        = cityTerritoryCanon;
            CityPossessionCanon       = cityPossessionCanon;

            improvementSignals.ImprovementRemovedFromLocationSignal .Subscribe(OnImprovementRemovedFromLocation);
            improvementSignals.ImprovementPillagedSignal            .Subscribe(OnImprovementPillaged);
            resourceSignals   .ResourceNodeRemovedFromLocationSignal.Subscribe(OnResourceNodeRemovedFromLocation);
            citySignals       .LostCellFromBoundariesSignal         .Subscribe(OnCityLostCellFromBoundaries);
            civSignals        .CivLosingCitySignal                  .Subscribe(OnCivLosingCity);
        }

        #endregion

        #region instance methods

        private void OnImprovementRemovedFromLocation(Tuple<IImprovement, IHexCell> dataTuple) {
            var location = dataTuple.Item2;

            var nodeAtLocation = ResourceNodeLocationCanon.GetPossessionsOfOwner(location).FirstOrDefault();

            if(nodeAtLocation == null) {
                return;
            }

            var civOwningCell = CivTerritoryLogic.GetCivClaimingCell(location);

            if(civOwningCell != null) {
                ResourceTransferCanon.SynchronizeResourceForCiv(nodeAtLocation.Resource, civOwningCell);
            }
        }

        private void OnImprovementPillaged(IImprovement improvement) {
            var location = ImprovementLocationCanon.GetOwnerOfPossession(improvement);

            var nodeAtLocation = ResourceNodeLocationCanon.GetPossessionsOfOwner(location).FirstOrDefault();

            if(nodeAtLocation == null) {
                return;
            }

            var civOwningCell = CivTerritoryLogic.GetCivClaimingCell(location);

            if(civOwningCell != null) {
                ResourceTransferCanon.SynchronizeResourceForCiv(nodeAtLocation.Resource, civOwningCell);
            }
        }

        private void OnResourceNodeRemovedFromLocation(Tuple<IResourceNode, IHexCell> dataTuple) {
            var node     = dataTuple.Item1;
            var location = dataTuple.Item2;

            var civOwningCell = CivTerritoryLogic.GetCivClaimingCell(location);

            if(civOwningCell != null) {
                ResourceTransferCanon.SynchronizeResourceForCiv(node.Resource, civOwningCell);
            }
        }

        private void OnCityLostCellFromBoundaries(Tuple<ICity, IHexCell> dataTuple) {
            var city = dataTuple.Item1;
            var cell = dataTuple.Item2;

            var nodeAtLocation = ResourceNodeLocationCanon.GetPossessionsOfOwner(cell).FirstOrDefault();

            if(nodeAtLocation == null) {
                return;
            }

            var cityOwner = CityPossessionCanon.GetOwnerOfPossession(city);

            ResourceTransferCanon.SynchronizeResourceForCiv(nodeAtLocation.Resource, cityOwner);
        }

        private void OnCivLosingCity(Tuple<ICivilization, ICity> dataTuple) {
            var oldOwner = dataTuple.Item1;
            var city     = dataTuple.Item2;

            foreach(var cell in CityTerritoryCanon.GetPossessionsOfOwner(city)) {
                var nodeAtLocation = ResourceNodeLocationCanon.GetPossessionsOfOwner(cell).FirstOrDefault();

                if(nodeAtLocation == null) {
                    continue;
                }

                ResourceTransferCanon.SynchronizeResourceForCiv(nodeAtLocation.Resource, oldOwner);
            }
        }

        #endregion

    }

}
