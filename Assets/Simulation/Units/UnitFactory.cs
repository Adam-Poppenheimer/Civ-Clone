﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using UniRx;

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

        #endregion

        #region constructors

        [Inject]
        public UnitFactory(DiContainer container, IUnitPositionCanon unitPositionCanon,
            IPossessionRelationship<ICivilization, IUnit> unitPossessionCanon,
            UnitSignals signals
        ){
            Container           = container;
            UnitPositionCanon   = unitPositionCanon;
            UnitPossessionCanon = unitPossessionCanon;

            signals.UnitBeingDestroyedSignal.Subscribe(OnUnitBeingDestroyed);
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

            var newUnitObject = Container.InstantiatePrefab(template.Prefab);

            newUnitObject.transform.SetParent(location.transform, false);

            var newUnit = newUnitObject.GetComponent<GameUnit>();
            newUnit.Template = template;

            newUnit.CurrentMovement = template.MaxMovement;

            if(UnitPossessionCanon.CanChangeOwnerOfPossession(newUnit, owner)) {
                UnitPossessionCanon.ChangeOwnerOfPossession(newUnit, owner);
            }else {
                throw new UnitCreationException("The newly created unit cannot be assigned to its owner");
            }

            if(UnitPositionCanon.CanChangeOwnerOfPossession(newUnit, location)) {
                UnitPositionCanon.ChangeOwnerOfPossession(newUnit, location);
            }else {
                throw new UnitCreationException("The newly created unit cannot be placed at its location");
            }
            
            var meshRenderer = newUnit.GetComponentInChildren<MeshRenderer>();
            if(meshRenderer != null) {
                meshRenderer.material.color = owner.Color;
            }

            allUnits.Add(newUnit);

            return newUnit;
        }

        #endregion

        private void OnUnitBeingDestroyed(IUnit unit) {
            allUnits.Remove(unit);
        }

        #endregion

    }

}
