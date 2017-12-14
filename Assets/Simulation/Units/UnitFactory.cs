using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.GameMap;

namespace Assets.Simulation.Units {

    public class UnitFactory : IUnitFactory {

        #region instance fields and properties

        #region from IUnitFactory

        public IEnumerable<IUnit> AllUnits {
            get { return allUnits.AsReadOnly(); }
        }
        private List<IUnit> allUnits = new List<IUnit>();

        #endregion

        private DiContainer Container;

        private IUnitPositionCanon UnitPositionCanon;

        private GameObject UnitPrefab;

        #endregion

        #region constructors

        [Inject]
        public UnitFactory(DiContainer container, IUnitPositionCanon unitPositionCanon,
            [Inject(Id = "Unit Prefab")] GameObject unitPrefab
        ){
            Container = container;
            UnitPositionCanon = unitPositionCanon;
            UnitPrefab = unitPrefab;
        }

        #endregion

        #region instance methods

        #region from IUnitFactory

        public IUnit Create(IMapTile location, IUnitTemplate template) {
            var newUnitObject = GameObject.Instantiate(UnitPrefab);
            Container.InjectGameObject(newUnitObject);

            newUnitObject.transform.SetParent(location.transform, false);

            var newUnit = newUnitObject.GetComponent<GameUnit>();
            newUnit.Template = template;

            newUnit.CurrentMovement = template.MaxMovement;

            UnitPositionCanon.ChangeOwnerOfPossession(newUnit, location);

            allUnits.Add(newUnit);

            return newUnit;
        }

        #endregion

        #endregion

    }

}
