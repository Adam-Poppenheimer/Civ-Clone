﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.EventSystems;

using Zenject;
using UniRx;

using Assets.Simulation.GameMap;
using Assets.Simulation.Units.Abilities;

namespace Assets.Simulation.Units {

    public class UnitInstaller : MonoInstaller {

        #region instance fields and properties

        [SerializeField] private UnitConfig UnitConfig;

        [SerializeField] private GameObject UnitPrefab;

        [SerializeField] private List<TerrainType> LandTerrainTypes;

        [SerializeField] private List<TerrainShape> ImpassableTerrainShapes;

        [SerializeField] private List<UnitTemplate> AvailableTemplates;

        #endregion

        #region instance methods

        #region from MonoInstaller

        public override void InstallBindings() {
            Container.Bind<IUnitConfig>().To<UnitConfig>().FromInstance(UnitConfig);

            Container.Bind<GameObject>().WithId("Unit Prefab").FromInstance(UnitPrefab);

            Container.Bind<IEnumerable<IUnitTemplate>>()
                .WithId("Available Unit Templates")
                .FromInstance(AvailableTemplates.Cast<IUnitTemplate>());

            Container.Bind<List<TerrainType>> ().WithId("Land Terrain Types")       .FromInstance(LandTerrainTypes);
            Container.Bind<List<TerrainShape>>().WithId("Impassable Terrain Shapes").FromInstance(ImpassableTerrainShapes);

            Container.Bind<IUnitFactory>().To<UnitFactory>().AsSingle();

            Container.Bind<IUnitPositionCanon>          ().To<UnitPositionCanon>          ().AsSingle();
            Container.Bind<IUnitProductionValidityLogic>().To<UnitProductionValidityLogic>().AsSingle();
            Container.Bind<IUnitTerrainCostLogic>       ().To<UnitTerrainCostLogic>       ().AsSingle();

            var unitClickedSignal = new Subject<IUnit>();

            var unitBeginDragSignal = new Subject<Tuple<IUnit, PointerEventData>>();
            var unitDragSignal      = new Subject<Tuple<IUnit, PointerEventData>>();
            var unitEndDragSignal   = new Subject<Tuple<IUnit, PointerEventData>>();

            Container.Bind<ISubject<IUnit>>().WithId("Unit Clicked Signal").FromInstance(unitClickedSignal);

            Container.Bind<ISubject<Tuple<IUnit, PointerEventData>>>().WithId("Unit Begin Drag Signal").FromInstance(unitBeginDragSignal);
            Container.Bind<ISubject<Tuple<IUnit, PointerEventData>>>().WithId("Unit Drag Signal"      ).FromInstance(unitDragSignal);
            Container.Bind<ISubject<Tuple<IUnit, PointerEventData>>>().WithId("Unit End Drag Signal"  ).FromInstance(unitEndDragSignal);

            Container.Bind<UnitSignals>().AsSingle();
        }

        #endregion

        #endregion

    }

}
