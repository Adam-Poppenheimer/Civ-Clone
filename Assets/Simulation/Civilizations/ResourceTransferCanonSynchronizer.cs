using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using UniRx;

using Assets.Simulation.Cities;
using Assets.Simulation.Improvements;
using Assets.Simulation.SpecialtyResources;
using Assets.Simulation.HexMap;

namespace Assets.Simulation.Civilizations {

    public class ResourceTransferCanonSynchronizer {

        #region instance fields and properties

        private IResourceTransferCanon                           ResourceTransferCanon;
        private IImprovementLocationCanon                        ImprovementLocationCanon;
        private IPossessionRelationship<IHexCell, IResourceNode> ResourceNodeLocationCanon;
        private ICivilizationTerritoryLogic                      CivTerritoryLogic;
        private IPossessionRelationship<ICity, IHexCell>         CityTerritoryCanon;

        #endregion

        #region constructors

        public ResourceTransferCanonSynchronizer(
            IResourceTransferCanon                           resourceTransferCanon,
            IImprovementLocationCanon                        improvementLocationCanon,
            IPossessionRelationship<IHexCell, IResourceNode> resourceNodeLocationCanon,
            ICivilizationTerritoryLogic                      civTerritoryLogic,
            IPossessionRelationship<ICity, IHexCell>         cityTerritoryCanon,
            ImprovementSignals                               improvementSignals,
            SpecialtyResourceSignals                         resourceSignals,
            CitySignals                                      citySignals,
            CivilizationSignals                              civSignals
        ) {
            ResourceTransferCanon     = resourceTransferCanon;
            ImprovementLocationCanon  = improvementLocationCanon;
            ResourceNodeLocationCanon = resourceNodeLocationCanon;
            CivTerritoryLogic         = civTerritoryLogic;
            CityTerritoryCanon        = cityTerritoryCanon;

            improvementSignals.ImprovementBeingRemovedFromLocationSignal.Subscribe(
                dataTuple => TrySynchronizeFromCell(dataTuple.Item2)
            );

            improvementSignals.ImprovementBeingPillagedSignal.Subscribe(
                improvement => TrySynchronizeFromCell(ImprovementLocationCanon.GetOwnerOfPossession(improvement))
            );

            resourceSignals.ResourceNodeBeingRemovedFromLocationSignal.Subscribe(
                dataTuple => TrySynchronizeFromCell(dataTuple.Item2)
            );

            citySignals.LostCellFromBoundariesSignal.Subscribe(
                dataTuple => TrySynchronizeFromCell(dataTuple.Item2)
            );

            civSignals.CivLosingCitySignal.Subscribe(delegate(Tuple<ICivilization, ICity> dataTuple) {
                foreach(var cell in CityTerritoryCanon.GetPossessionsOfOwner(dataTuple.Item2)) {
                    TrySynchronizeFromCell(cell);
                }
            });
        }

        #endregion

        #region instance methods

        private void TrySynchronizeFromCell(IHexCell cell) {
            var nodeAtLocation = ResourceNodeLocationCanon.GetPossessionsOfOwner(cell).FirstOrDefault();

            if(nodeAtLocation == null) {
                return;
            }

            var civOwningCell = CivTerritoryLogic.GetCivClaimingCell(cell);

            if(civOwningCell != null) {
                ResourceTransferCanon.SynchronizeResourceForCiv(nodeAtLocation.Resource, civOwningCell);
            }
        }

        #endregion

    }

}
