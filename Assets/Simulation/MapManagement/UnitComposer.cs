using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.Units;
using Assets.Simulation.HexMap;
using Assets.Simulation.Cities;
using Assets.Simulation.Civilizations;

namespace Assets.Simulation.MapManagement {

    public class UnitComposer {

        #region instance fields and properties

        private IUnitFactory                                  UnitFactory;
        private IPossessionRelationship<ICivilization, IUnit> UnitPossessionCanon;
        private IUnitPositionCanon                            UnitPositionCanon;
        private IHexGrid                                      Grid;
        private ICivilizationFactory                          CivilizationFactory;
        private IEnumerable<IUnitTemplate>                    AvailableUnitTemplates;

        #endregion

        #region constructors

        [Inject]
        public UnitComposer(
            IUnitFactory unitFactory, IPossessionRelationship<ICivilization, IUnit> unitPossessionCanon,
            IUnitPositionCanon unitPositionCanon, IHexGrid grid, ICivilizationFactory civilizationFactory,
            [Inject(Id = "Available Unit Templates")] IEnumerable<IUnitTemplate> availableUnitTemplates
        ) {
            UnitFactory            = unitFactory;
            UnitPossessionCanon    = unitPossessionCanon;
            UnitPositionCanon      = unitPositionCanon;
            Grid                   = grid;
            CivilizationFactory    = civilizationFactory;
            AvailableUnitTemplates = availableUnitTemplates;
        }

        #endregion

        #region instance methosd

        public void ClearRuntime() {
            foreach(var unit in new List<IUnit>(UnitFactory.AllUnits)) {
                GameObject.DestroyImmediate(unit.gameObject);
            }
        }

        public void ComposeUnits(SerializableMapData mapData) {
            mapData.Units = new List<SerializableUnitData>();

            foreach(var unit in UnitFactory.AllUnits) {
                if(unit is CityCombatFacadeUnit) {
                    continue;
                }

                var unitOwner    = UnitPossessionCanon.GetOwnerOfPossession(unit);
                var unitLocation = UnitPositionCanon.GetOwnerOfPossession(unit);

                var unitData = new SerializableUnitData() {
                    Location        = unitLocation.Coordinates,
                    Template        = unit.Template.Name,
                    Owner           = unitOwner.Name,
                    CurrentMovement = unit.CurrentMovement,
                    Health          = unit.Health,
                    CurrentPath     = unit.CurrentPath != null ? unit.CurrentPath.Select(cell => cell.Coordinates).ToList() : null,
                };

                mapData.Units.Add(unitData);
            }
        }

        public void DecomposeUnits(SerializableMapData mapData) {
            foreach(var unitData in mapData.Units) {
                var unitLocation    = Grid.GetCellAtCoordinates(unitData.Location);
                var templateToBuild = AvailableUnitTemplates.Where(template => template.Name.Equals(unitData.Template)).First();
                var unitOwner       = CivilizationFactory.AllCivilizations.Where(civ => civ.Name.Equals(unitData.Owner)).First();                

                var newUnit = UnitFactory.Create(unitLocation, templateToBuild, unitOwner);

                newUnit.CurrentMovement = unitData.CurrentMovement;
                newUnit.Health          = unitData.Health;

                if(unitData.CurrentPath != null) {
                    newUnit.CurrentPath = unitData.CurrentPath.Select(coord => Grid.GetCellAtCoordinates(coord)).ToList();
                }
            }
        }

        #endregion

    }

}
