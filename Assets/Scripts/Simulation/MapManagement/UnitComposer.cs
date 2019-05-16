﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Units;
using Assets.Simulation.HexMap;
using Assets.Simulation.Civilizations;

namespace Assets.Simulation.MapManagement {

    public class UnitComposer : IUnitComposer {

        #region instance fields and properties

        private IUnitFactory                                  UnitFactory;
        private IPossessionRelationship<ICivilization, IUnit> UnitPossessionCanon;
        private IUnitPositionCanon                            UnitPositionCanon;
        private IHexGrid                                      Grid;
        private ICivilizationFactory                          CivilizationFactory;
        private IEnumerable<IUnitTemplate>                    AvailableUnitTemplates;
        private IPromotionTreeComposer                        PromotionTreeComposer;

        #endregion

        #region constructors

        [Inject]
        public UnitComposer(
            IUnitFactory unitFactory, IPossessionRelationship<ICivilization, IUnit> unitPossessionCanon,
            IUnitPositionCanon unitPositionCanon, IHexGrid grid, ICivilizationFactory civilizationFactory,
            [Inject(Id = "Available Unit Templates")] IEnumerable<IUnitTemplate> availableUnitTemplates,
            IPromotionTreeComposer promotionTreeComposer
        ) {
            UnitFactory            = unitFactory;
            UnitPossessionCanon    = unitPossessionCanon;
            UnitPositionCanon      = unitPositionCanon;
            Grid                   = grid;
            CivilizationFactory    = civilizationFactory;
            AvailableUnitTemplates = availableUnitTemplates;
            PromotionTreeComposer  = promotionTreeComposer;
        }

        #endregion

        #region instance methosd

        public void ClearRuntime() {
            foreach(var unit in new List<IUnit>(UnitFactory.AllUnits)) {
                UnitPositionCanon.ChangeOwnerOfPossession(unit, null);
                unit.Destroy();
            }
        }

        public void ComposeUnits(SerializableMapData mapData) {
            mapData.Units = new List<SerializableUnitData>();

            foreach(var unit in UnitFactory.AllUnits) {
                if(unit.Type == UnitType.City) {
                    continue;
                }

                var unitOwner    = UnitPossessionCanon.GetOwnerOfPossession(unit);
                var unitLocation = UnitPositionCanon.GetOwnerOfPossession(unit);

                var unitData = new SerializableUnitData() {
                    Location         = unitLocation.Coordinates,
                    Template         = unit.Template.name,
                    Owner            = unitOwner.Template.Name,
                    CurrentMovement  = unit.CurrentMovement,
                    Hitpoints        = unit.CurrentHitpoints,
                    CurrentPath      = unit.CurrentPath != null ? unit.CurrentPath.Select(cell => cell.Coordinates).ToList() : null,
                    IsSetUpToBombard = unit.IsSetUpToBombard,
                    Experience       = unit.Experience,
                    Level            = unit.Level,
                    PromotionTree    = PromotionTreeComposer.ComposePromotionTree(unit.PromotionTree)
                };

                mapData.Units.Add(unitData);
            }
        }

        public void DecomposeUnits(SerializableMapData mapData) {
            foreach(var unitData in mapData.Units) {
                var unitLocation    = Grid.GetCellAtCoordinates(unitData.Location);
                var templateToBuild = AvailableUnitTemplates.Where(template => template.name.Equals(unitData.Template)).First();
                var unitOwner       = CivilizationFactory.AllCivilizations.Where(civ => civ.Template.Name.Equals(unitData.Owner)).First();    
                var promotionTree   = PromotionTreeComposer.DecomposePromotionTree(unitData.PromotionTree);            

                var newUnit = UnitFactory.BuildUnit(unitLocation, templateToBuild, unitOwner, promotionTree);

                newUnit.CurrentMovement = unitData.CurrentMovement;
                newUnit.CurrentHitpoints       = unitData.Hitpoints;
                newUnit.Experience      = unitData.Experience;
                newUnit.Level           = unitData.Level;

                if(unitData.CurrentPath != null) {
                    newUnit.CurrentPath = unitData.CurrentPath.Select(coord => Grid.GetCellAtCoordinates(coord)).ToList();
                }

                if(unitData.IsSetUpToBombard) {
                    newUnit.SetUpToBombard();
                }
            }
        }

        #endregion

    }

}
