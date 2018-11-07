using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using UniRx;

using Assets.Simulation.WorkerSlots;

namespace Assets.Simulation.Cities.Buildings {

    public class BuildingFactory : IBuildingFactory {

        #region instance fields and properties

        #region from IBuildingFactory

        public IEnumerable<IBuilding> AllBuildings {
            get { return allBuildings; }
        }
        private List<IBuilding> allBuildings = new List<IBuilding>();

        #endregion

        private IPossessionRelationship<ICity, IBuilding> PossessionCanon;
        private IWorkerSlotFactory                        WorkerSlotFactory;

        #endregion

        #region constructors

        [Inject]
        public BuildingFactory(
            IPossessionRelationship<ICity, IBuilding> possessionCanon, IWorkerSlotFactory workerSlotFactory,
            CitySignals citySignals
        ){
            PossessionCanon   = possessionCanon;
            WorkerSlotFactory = workerSlotFactory;

            citySignals.CityBeingDestroyedSignal.Subscribe(OnCityBeingDestroyed);
        }

        #endregion

        #region instance methods

        #region from IBuildingFactory

        public IBuilding BuildBuilding(IBuildingTemplate template, ICity city) {
            if(template == null) {
                throw new ArgumentNullException("template");
            }else if(city == null) {
                throw new ArgumentNullException("city");
            }

            var newBuilding = new Building(template);

            var slots = new List<IWorkerSlot>();
            for(int i = 0; i < template.SlotCount; i++) {
                slots.Add(WorkerSlotFactory.BuildSlot(newBuilding));
            }

            newBuilding.Slots = slots.AsReadOnly();

            if(!PossessionCanon.CanChangeOwnerOfPossession(newBuilding, city)) {
                throw new BuildingCreationException("The building produced from this template cannot be placed into this city");
            }
            PossessionCanon.ChangeOwnerOfPossession(newBuilding, city);

            allBuildings.Add(newBuilding);

            return newBuilding;
        }

        public void DestroyBuilding(IBuilding building) {
            PossessionCanon.ChangeOwnerOfPossession(building, null);

            allBuildings.Remove(building);
        }

        #endregion

        private void OnCityBeingDestroyed(ICity city) {
            foreach(var building in PossessionCanon.GetPossessionsOfOwner(city).ToArray()) {
                DestroyBuilding(building);
            }
        }

        #endregion

    }

}
