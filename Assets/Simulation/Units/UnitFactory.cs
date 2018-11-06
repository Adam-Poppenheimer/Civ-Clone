using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using UniRx;

using Assets.Simulation.HexMap;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Units.Promotions;
using Assets.Simulation.Units.Combat;

namespace Assets.Simulation.Units {

    public class UnitFactory : IUnitFactory {

        #region instance fields and properties

        #region from IUnitFactory

        public IEnumerable<IUnit> AllUnits {
            get { return allUnits.AsReadOnly(); }
        }
        private List<IUnit> allUnits = new List<IUnit>();

        #endregion

        private DiContainer                                   Container;
        private IUnitPositionCanon                            UnitPositionCanon;
        private IPossessionRelationship<ICivilization, IUnit> UnitPossessionCanon;
        private Transform                                     UnitContainer;
        private IUnitConfig                                   UnitConfig;

        #endregion

        #region constructors

        [Inject]
        public UnitFactory(
            DiContainer container, IUnitPositionCanon unitPositionCanon,
            IPossessionRelationship<ICivilization, IUnit> unitPossessionCanon,
            [Inject(Id = "Unit Container")] Transform unitContainer,
            IUnitConfig unitConfig, UnitSignals signals
        ){
            Container           = container;
            UnitPositionCanon   = unitPositionCanon;
            UnitPossessionCanon = unitPossessionCanon;
            UnitContainer       = unitContainer;
            UnitConfig          = unitConfig;

            signals.UnitBeingDestroyedSignal.Subscribe(OnUnitBeingDestroyed);
        }

        #endregion

        #region instance methods

        #region from IUnitFactory

        public bool CanBuildUnit(IHexCell location, IUnitTemplate template, ICivilization owner) {
            var canPlace = UnitPositionCanon.CanPlaceUnitTemplateAtLocation(template, location, owner);
            var hasForeignUnits = DoesCellHaveUnitsForeignTo(location, owner);;

            return canPlace && !hasForeignUnits;
        }

        public IUnit BuildUnit(IHexCell location, IUnitTemplate template, ICivilization owner) {
            if(template == null) {
                throw new ArgumentNullException("template");
            }

            return BuildUnit(location, template, owner, new PromotionTree(template.PromotionTreeData));
        }

        public IUnit BuildUnit(IHexCell location, IUnitTemplate template, ICivilization owner, IPromotionTree promotionTree) {
            if(location == null) {
                throw new ArgumentNullException("location");
            }else if(template == null) {
                throw new ArgumentNullException("template");
            }else if(owner == null) {
                throw new ArgumentNullException("owner");
            }

            var newUnitObject  = GameObject.Instantiate(UnitConfig.UnitPrefab);
            var newUnitDisplay = GameObject.Instantiate(template.DisplayPrefab);

            newUnitDisplay.transform.SetParent(newUnitObject.transform);

            Container.InjectGameObject(newUnitObject);            

            var newUnit = newUnitObject.GetComponent<GameUnit>();

            newUnit.transform.SetParent(UnitContainer, false);

            newUnit.Template = template;

            newUnit.CurrentMovement  = template.MaxMovement;
            newUnit.CurrentHitpoints = newUnit.MaxHitpoints;
            newUnit.CanAttack        = true;
            newUnit.Level            = 1;
            newUnit.PromotionTree    = promotionTree;

            newUnit.SetSummaries(new UnitMovementSummary(), new UnitCombatSummary());

            allUnits.Add(newUnit);

            if(UnitPossessionCanon.CanChangeOwnerOfPossession(newUnit, owner)) {
                UnitPossessionCanon.ChangeOwnerOfPossession(newUnit, owner);
            }else {
                throw new UnitCreationException("The newly created unit cannot be assigned to its owner");
            }

            if(newUnit.CanRelocate(location)) {
                newUnit.Relocate(location);
            }else {
                throw new UnitCreationException("The newly created unit cannot be placed at its location");
            }
            
            var meshRenderer = newUnit.GetComponentInChildren<MeshRenderer>();
            if(meshRenderer != null) {
                meshRenderer.material.color = owner.Color;
            }

            return newUnit;
        }

        #endregion

        private bool DoesCellHaveUnitsForeignTo(IHexCell cell, ICivilization domesticCiv) {
            foreach(var unit in UnitPositionCanon.GetPossessionsOfOwner(cell)) {
                if(UnitPossessionCanon.GetOwnerOfPossession(unit) != domesticCiv) {
                    return true;
                }
            }

            return false;
        }

        private void OnUnitBeingDestroyed(IUnit unit) {
            allUnits.Remove(unit);
        }

        #endregion

    }

}
