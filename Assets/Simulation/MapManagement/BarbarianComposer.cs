using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Barbarians;
using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapManagement {

    public class BarbarianComposer : IBarbarianComposer {

        #region instance fields and properties

        private IEncampmentFactory       EncampmentFactory;
        private IEncampmentLocationCanon EncampmentLocationCanon;
        private IHexGrid                 Grid;

        #endregion

        #region constructors

        [Inject]
        public BarbarianComposer(
            IEncampmentFactory encampmentFactory, IEncampmentLocationCanon encampmentLocationCanon,
            IHexGrid grid
        ) {
            EncampmentFactory       = encampmentFactory;
            EncampmentLocationCanon = encampmentLocationCanon;
            Grid                    = grid;
        }

        #endregion

        #region instance methods

        #region from IBarbarianComposer

        public void ClearRuntime() {
            foreach(var encampment in EncampmentFactory.AllEncampments.ToArray()) {
                EncampmentFactory.DestroyEncampment(encampment);
            }
        }

        public void ComposeBarbarians(SerializableMapData mapData) {
            mapData.Encampments = new List<SerializableEncampmentData>();

            foreach(var encampment in EncampmentFactory.AllEncampments) {
                var location = EncampmentLocationCanon.GetOwnerOfPossession(encampment);

                var encampmentData = new SerializableEncampmentData() {
                    Location      = location.Coordinates,
                    SpawnProgress = encampment.SpawnProgress
                };

                mapData.Encampments.Add(encampmentData);
            }
        }

        public void DecomposeBarbarians(SerializableMapData mapData) {
            foreach(var encampmentData in mapData.Encampments) {
                var location = Grid.GetCellAtCoordinates(encampmentData.Location);

                var newEncampment = EncampmentFactory.CreateEncampment(location);

                newEncampment.SpawnProgress = encampmentData.SpawnProgress;
            }
        }

        #endregion

        #endregion

    }

}
