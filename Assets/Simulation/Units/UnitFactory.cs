using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Simulation.Civilizations;

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

        private IPossessionRelationship<ICivilization, IUnit> UnitPossessionCanon;

        private GameObject UnitPrefab;

        #endregion

        #region constructors

        [Inject]
        public UnitFactory(DiContainer container, IUnitPositionCanon unitPositionCanon,
            IPossessionRelationship<ICivilization, IUnit> unitPossessionCanon,
            [Inject(Id = "Unit Prefab")] GameObject unitPrefab
        ){
            Container           = container;
            UnitPositionCanon   = unitPositionCanon;
            UnitPossessionCanon = unitPossessionCanon;
            UnitPrefab          = unitPrefab;
        }

        #endregion

        #region instance methods

        #region from IUnitFactory

        public IUnit Create(IHexCell location, IUnitTemplate template, ICivilization owner) {
            if(location == null) {
                throw new ArgumentNullException("location");
            }else if(template == null) {
                throw new ArgumentNullException("template");
            }else if(owner == null) {
                throw new ArgumentNullException("owner");
            }

            var newUnitObject = GameObject.Instantiate(UnitPrefab);
            Container.InjectGameObject(newUnitObject);

            newUnitObject.transform.SetParent(location.transform, false);

            var newUnit = newUnitObject.GetComponent<GameUnit>();
            newUnit.Template = template;

            newUnit.CurrentMovement = template.MaxMovement;

            if(UnitPositionCanon.CanChangeOwnerOfPossession(newUnit, location)) {
                UnitPositionCanon.ChangeOwnerOfPossession(newUnit, location);
            }else {
                throw new UnitCreationException("The newly created unit cannot be placed at its location");
            }
            
            if(UnitPossessionCanon.CanChangeOwnerOfPossession(newUnit, owner)) {
                UnitPossessionCanon.ChangeOwnerOfPossession(newUnit, owner);
            }else {
                throw new UnitCreationException("The newly created unit cannot be assigned to its owner");
            }
            
            newUnit.GetComponentInChildren<MeshRenderer>().material.color = owner.Color;

            allUnits.Add(newUnit);

            return newUnit;
        }

        #endregion

        #endregion

    }

}
