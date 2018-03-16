using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation;

using Assets.Simulation.HexMap;

using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.Cities.ResourceGeneration;
using Assets.Simulation.Civilizations;
using Assets.Simulation.WorkerSlots;

namespace Assets.Tests.Simulation.Cities {

    [TestFixture]
    public class ResourceGenerationLogicTests : ZenjectUnitTestFixture {

        #region internal types

        public class CityResourceTestData {

            public CityTestData City;

            public CityConfigTestData Config;

        }

        public class SlotResourceTestData {

            public SlotTestData Slot;

            public CityTestData City;

        }

        public class CityTestData {

            public int Population;

            public CellTestData Location;

            public List<CellTestData> NonLocationTerritory;

            public List<BuildingTestData> Buildings;

            public CivilizationTestData OwningCivilization;

            public ResourceSummary CityYieldMultipliers;

        }

        public class CellTestData {

            public SlotTestData Slot;

            public bool SuppressSlot;

        }

        public class BuildingTestData {

            public ResourceSummary StaticYield;

            public List<SlotTestData> Slots;

            public ResourceSummary BonusYieldPerPopulation;

        }

        public class SlotTestData {

            public ResourceSummary BaseYield;

            public bool IsOccupied;

            public ResourceSummary YieldMultipliers;

        }

        public class CivilizationTestData {

            public ResourceSummary YieldMultipliers;

        }

        public class CityConfigTestData {

            public ResourceSummary UnemployedYield;

        }

        #endregion

        #region static fields and properties

        public static IEnumerable GetTotalYieldForCityTestCases {
            get {
                yield return new TestCaseData(new CityResourceTestData() {
                    City = new CityTestData() {
                        Location = new CellTestData() {
                            Slot = new SlotTestData() { BaseYield = new ResourceSummary(food: 5, production: 4) },
                            SuppressSlot = true
                        },
                        Buildings = new List<BuildingTestData>() { },
                        CityYieldMultipliers = new ResourceSummary(),
                        NonLocationTerritory = new List<CellTestData>() { },
                        OwningCivilization = new CivilizationTestData() {
                            YieldMultipliers = new ResourceSummary()
                        },
                        Population = 0
                    },
                    Config = new CityConfigTestData() {
                        UnemployedYield = new ResourceSummary()
                    }
                }).SetName("Always considers base yield of location").Returns(new ResourceSummary(food: 5, production: 4));

                yield return new TestCaseData(new CityResourceTestData() {
                    City = new CityTestData() {
                        Location = new CellTestData() {
                            Slot = new SlotTestData() { BaseYield = new ResourceSummary(food: 5, production: 4) },
                            SuppressSlot = true
                        },
                        Buildings = new List<BuildingTestData>() { },
                        CityYieldMultipliers = new ResourceSummary(food: -0.5f, production: 1f),
                        NonLocationTerritory = new List<CellTestData>() { },
                        OwningCivilization = new CivilizationTestData() {
                            YieldMultipliers = new ResourceSummary(food: 1f, production: 2f)
                        },
                        Population = 0
                    },
                    Config = new CityConfigTestData() {
                        UnemployedYield = new ResourceSummary()
                    }

                }).SetName("Location yield modified by city and civilization modifiers").Returns(
                    new ResourceSummary(new ResourceSummary(food: 5 * 1.5f, production: 16f))
                );

                yield return new TestCaseData(new CityResourceTestData() {
                    City = new CityTestData() {
                        Location = new CellTestData() {
                            Slot = new SlotTestData() { BaseYield = new ResourceSummary() },
                            SuppressSlot = true
                        },
                        Buildings = new List<BuildingTestData>() { },
                        CityYieldMultipliers = new ResourceSummary(),
                        NonLocationTerritory = new List<CellTestData>() {
                            new CellTestData() {
                                Slot = new SlotTestData() { BaseYield = new ResourceSummary(food: 1), IsOccupied = true },
                            },
                            new CellTestData() {
                                Slot = new SlotTestData() { BaseYield = new ResourceSummary(production: 2), IsOccupied = false },
                            },
                            new CellTestData() {
                                Slot = new SlotTestData() { BaseYield = new ResourceSummary(gold: 3), IsOccupied = true },
                            },
                            new CellTestData() {
                                Slot = new SlotTestData() { BaseYield = new ResourceSummary(culture: 4), IsOccupied = false },
                            }
                        },
                        OwningCivilization = new CivilizationTestData() {
                            YieldMultipliers = new ResourceSummary()
                        },
                        Population = 0
                    },
                    Config = new CityConfigTestData() {
                        UnemployedYield = new ResourceSummary()
                    }
                }).SetName("Adds yield of occupied cells, but not unoccupied ones").Returns(new ResourceSummary(food: 1, gold: 3));

                yield return new TestCaseData(new CityResourceTestData() {
                    City = new CityTestData() {
                        Location = new CellTestData() {
                            Slot = new SlotTestData() { BaseYield = new ResourceSummary() },
                            SuppressSlot = true
                        },
                        Buildings = new List<BuildingTestData>() { },
                        CityYieldMultipliers = new ResourceSummary(),
                        NonLocationTerritory = new List<CellTestData>() {
                            new CellTestData() {
                                Slot = new SlotTestData() { BaseYield = new ResourceSummary(food: 1), IsOccupied = true },
                            },
                            new CellTestData() {
                                Slot = new SlotTestData() { BaseYield = new ResourceSummary(production: 2), IsOccupied = false },
                            },
                            new CellTestData() {
                                Slot = new SlotTestData() { BaseYield = new ResourceSummary(gold: 3), IsOccupied = true },
                                SuppressSlot = true
                            },
                            new CellTestData() {
                                Slot = new SlotTestData() { BaseYield = new ResourceSummary(culture: 4), IsOccupied = false },
                            }
                        },
                        OwningCivilization = new CivilizationTestData() {
                            YieldMultipliers = new ResourceSummary()
                        },
                        Population = 0
                    },
                    Config = new CityConfigTestData() {
                        UnemployedYield = new ResourceSummary()
                    }
                }).SetName("Ignores yield of occupied but suppressed cells").Returns(new ResourceSummary(food: 1));

                yield return new TestCaseData(new CityResourceTestData() {
                    City = new CityTestData() {
                        Location = new CellTestData() {
                            Slot = new SlotTestData() { BaseYield = new ResourceSummary() },
                            SuppressSlot = true
                        },
                        Buildings = new List<BuildingTestData>() { },
                        CityYieldMultipliers = new ResourceSummary(food: -1, production: -1, gold: -1, culture: -1),
                        NonLocationTerritory = new List<CellTestData>() {
                            new CellTestData() {
                                Slot = new SlotTestData() {
                                    BaseYield = new ResourceSummary(food: 1), IsOccupied = true,
                                    YieldMultipliers = new ResourceSummary(food: 1f)
                                },
                            },
                            new CellTestData() {
                                Slot = new SlotTestData() {
                                    BaseYield = new ResourceSummary(production: 2), IsOccupied = false,
                                    YieldMultipliers = new ResourceSummary(production: 2f)
                                },
                            },
                            new CellTestData() {
                                Slot = new SlotTestData() {
                                    BaseYield = new ResourceSummary(gold: 3), IsOccupied = true,
                                    YieldMultipliers = new ResourceSummary(gold: 3f)
                                },
                            },
                            new CellTestData() {
                                Slot = new SlotTestData() {
                                    BaseYield = new ResourceSummary(culture: 4), IsOccupied = false,
                                    YieldMultipliers = new ResourceSummary(culture: 4f)
                                },
                            }
                        },
                        OwningCivilization = new CivilizationTestData() {
                            YieldMultipliers = new ResourceSummary(food: 0.5f, gold: 0.5f)
                        },
                        Population = 0
                    },
                    Config = new CityConfigTestData() {
                        UnemployedYield = new ResourceSummary()
                    }
                }).SetName("Cell slots consider slot, city, and civilization modifiers").Returns(
                    new ResourceSummary(food: 1.5f, gold: 3 * 3.5f)
                );

                yield return new TestCaseData(new CityResourceTestData() {
                    City = new CityTestData() {
                        Location = new CellTestData() {
                            Slot = new SlotTestData() { BaseYield = new ResourceSummary() },
                            SuppressSlot = true
                        },
                        Buildings = new List<BuildingTestData>() {
                            new BuildingTestData() {
                                StaticYield = new ResourceSummary(food: 2, production: 3),
                                Slots = new List<SlotTestData>() { }
                            },
                            new BuildingTestData() {
                                StaticYield = new ResourceSummary(production: 4, gold: 5),
                                Slots = new List<SlotTestData>() { }
                            },
                            new BuildingTestData() {
                                StaticYield = new ResourceSummary(gold: 6, culture: 7),
                                Slots = new List<SlotTestData>() { }
                            }
                        },
                        CityYieldMultipliers = new ResourceSummary(),
                        NonLocationTerritory = new List<CellTestData>() { },
                        OwningCivilization = new CivilizationTestData() {
                            YieldMultipliers = new ResourceSummary()
                        },
                        Population = 0
                    },
                    Config = new CityConfigTestData() {
                        UnemployedYield = new ResourceSummary()
                    }
                }).SetName("Adds static yield of all buildings").Returns(
                    new ResourceSummary(food: 2, production: 7, gold: 11, culture: 7)
                );

                yield return new TestCaseData(new CityResourceTestData() {
                    City = new CityTestData() {
                        Location = new CellTestData() {
                            Slot = new SlotTestData() { BaseYield = new ResourceSummary() },
                            SuppressSlot = true
                        },
                        Buildings = new List<BuildingTestData>() {
                            new BuildingTestData() {
                                StaticYield = new ResourceSummary(food: 2, production: 3),
                                Slots = new List<SlotTestData>() { }
                            },
                            new BuildingTestData() {
                                StaticYield = new ResourceSummary(production: 4, gold: 5),
                                Slots = new List<SlotTestData>() { }
                            },
                            new BuildingTestData() {
                                StaticYield = new ResourceSummary(gold: 6, culture: 7),
                                Slots = new List<SlotTestData>() { }
                            }
                        },
                        CityYieldMultipliers = new ResourceSummary(food: 1f, production: 1.5f, gold: -2f, culture: 2.5f),
                        NonLocationTerritory = new List<CellTestData>() { },
                        OwningCivilization = new CivilizationTestData() {
                            YieldMultipliers = new ResourceSummary(culture: -1.5f)
                        },
                        Population = 0
                    },
                    Config = new CityConfigTestData() {
                        UnemployedYield = new ResourceSummary()
                    }
                }).SetName("Static yield considers city and civilization modifiers").Returns(
                    new ResourceSummary(food: 4, production: 7 * 2.5f, gold: -11, culture: 14)
                );

                yield return new TestCaseData(new CityResourceTestData() {
                    City = new CityTestData() {
                        Location = new CellTestData() {
                            Slot = new SlotTestData() { BaseYield = new ResourceSummary() },
                            SuppressSlot = true
                        },
                        Buildings = new List<BuildingTestData>() {
                            new BuildingTestData() {
                                BonusYieldPerPopulation = new ResourceSummary(food: 2f),
                                Slots = new List<SlotTestData>() { }
                            },
                            new BuildingTestData() {
                                BonusYieldPerPopulation = new ResourceSummary(production: 2.5f),
                                Slots = new List<SlotTestData>() { }
                            },
                            new BuildingTestData() {
                                BonusYieldPerPopulation = new ResourceSummary(gold: 3.5f),
                                Slots = new List<SlotTestData>() { }
                            }
                        },
                        CityYieldMultipliers = new ResourceSummary(),
                        NonLocationTerritory = new List<CellTestData>() { },
                        OwningCivilization = new CivilizationTestData() {
                            YieldMultipliers = new ResourceSummary()
                        },
                        Population = 10
                    },
                    Config = new CityConfigTestData() {
                        UnemployedYield = new ResourceSummary()
                    }
                }).SetName("Adds per-population yield of all buildings").Returns(
                    new ResourceSummary(food: 2f * 10, production: 2.5f * 10, gold: 3.5f * 10, science: 10)
                );

                yield return new TestCaseData(new CityResourceTestData() {
                    City = new CityTestData() {
                        Location = new CellTestData() {
                            Slot = new SlotTestData() { BaseYield = new ResourceSummary() },
                            SuppressSlot = true
                        },
                        Buildings = new List<BuildingTestData>() {
                            new BuildingTestData() {
                                BonusYieldPerPopulation = new ResourceSummary(food: 2f),
                                Slots = new List<SlotTestData>() { }
                            },
                            new BuildingTestData() {
                                BonusYieldPerPopulation = new ResourceSummary(production: 2.5f),
                                Slots = new List<SlotTestData>() { }
                            },
                            new BuildingTestData() {
                                BonusYieldPerPopulation = new ResourceSummary(gold: 3.5f),
                                Slots = new List<SlotTestData>() { }
                            }
                        },
                        CityYieldMultipliers = new ResourceSummary(food: -1, production: -1, gold: -1),
                        NonLocationTerritory = new List<CellTestData>() { },
                        OwningCivilization = new CivilizationTestData() {
                            YieldMultipliers = new ResourceSummary(production: 1, gold: 2)
                        },
                        Population = 10
                    },
                    Config = new CityConfigTestData() {
                        UnemployedYield = new ResourceSummary()
                    }
                }).SetName("Per-population yield considers city and civilization modifiers").Returns(
                    new ResourceSummary(food: 0, production: 2.5f * 10, gold: 3.5f * 20, science: 10)
                );

                yield return new TestCaseData(new CityResourceTestData() {
                    City = new CityTestData() {
                        Location = new CellTestData() {
                            Slot = new SlotTestData() { BaseYield = new ResourceSummary() },
                            SuppressSlot = true
                        },
                        Buildings = new List<BuildingTestData>() {
                            new BuildingTestData() {
                                Slots = new List<SlotTestData>() {
                                    new SlotTestData() { BaseYield = new ResourceSummary(food: 1),       IsOccupied = true },
                                    new SlotTestData() { BaseYield = new ResourceSummary(production: 1), IsOccupied = false }
                                }
                            },
                            new BuildingTestData() {
                                Slots = new List<SlotTestData>() {
                                    new SlotTestData() { BaseYield = new ResourceSummary(gold: 1),    IsOccupied = false },
                                    new SlotTestData() { BaseYield = new ResourceSummary(culture: 1), IsOccupied = true }
                                }
                            }
                        },
                        CityYieldMultipliers = new ResourceSummary(),
                        NonLocationTerritory = new List<CellTestData>() { },
                        OwningCivilization = new CivilizationTestData() {
                            YieldMultipliers = new ResourceSummary()
                        },
                        Population = 0
                    },
                    Config = new CityConfigTestData() {
                        UnemployedYield = new ResourceSummary()
                    }
                }).SetName("Adds yield of occupied slots in all buildings").Returns(
                    new ResourceSummary(food: 1f, culture: 1f)
                );

                yield return new TestCaseData(new CityResourceTestData() {
                    City = new CityTestData() {
                        Location = new CellTestData() {
                            Slot = new SlotTestData() { BaseYield = new ResourceSummary() },
                            SuppressSlot = true
                        },
                        Buildings = new List<BuildingTestData>() {
                            new BuildingTestData() {
                                Slots = new List<SlotTestData>() {
                                    new SlotTestData() {
                                        BaseYield = new ResourceSummary(food: 1),
                                        IsOccupied = true,
                                        YieldMultipliers = new ResourceSummary(food: 1f)
                                    },
                                    new SlotTestData() {
                                        BaseYield = new ResourceSummary(production: 1),
                                        IsOccupied = false,
                                        YieldMultipliers = new ResourceSummary(production: 1f)
                                    }
                                }
                            },
                            new BuildingTestData() {
                                Slots = new List<SlotTestData>() {
                                    new SlotTestData() {
                                        BaseYield = new ResourceSummary(gold: 1),
                                        IsOccupied = true,
                                        YieldMultipliers = new ResourceSummary(gold: 1f)
                                    },
                                    new SlotTestData() {
                                        BaseYield = new ResourceSummary(culture: 1),
                                        IsOccupied = true,
                                        YieldMultipliers = new ResourceSummary(culture: 2f)
                                    }
                                }
                            }
                        },
                        CityYieldMultipliers = new ResourceSummary(food: -2f),
                        NonLocationTerritory = new List<CellTestData>() { },
                        OwningCivilization = new CivilizationTestData() {
                            YieldMultipliers = new ResourceSummary(culture: 5f)
                        },
                        Population = 0
                    },
                    Config = new CityConfigTestData() {
                        UnemployedYield = new ResourceSummary()
                    }
                }).SetName("Building slots consider slot, city, and civilization multipliers").Returns(
                    new ResourceSummary(food: 0, gold: 2f, culture: 8f)
                );

                yield return new TestCaseData(new CityResourceTestData() {
                    City = new CityTestData() {
                        Location = new CellTestData() {
                            Slot = new SlotTestData() { BaseYield = new ResourceSummary() },
                            SuppressSlot = true
                        },
                        Buildings = new List<BuildingTestData>() {
                            new BuildingTestData() {
                                Slots = new List<SlotTestData>() {
                                    new SlotTestData() { IsOccupied = true },
                                    new SlotTestData() { IsOccupied = true },
                                }
                            },
                            new BuildingTestData() {
                                Slots = new List<SlotTestData>() {
                                    new SlotTestData() { IsOccupied = true },
                                    new SlotTestData() { IsOccupied = false },
                                }
                            }
                        },
                        CityYieldMultipliers = new ResourceSummary(),
                        NonLocationTerritory = new List<CellTestData>() {
                            new CellTestData() { Slot = new SlotTestData() { IsOccupied = true } },
                            new CellTestData() { Slot = new SlotTestData() { IsOccupied = true } },
                            new CellTestData() { Slot = new SlotTestData() { IsOccupied = false } }
                        },
                        OwningCivilization = new CivilizationTestData() {
                            YieldMultipliers = new ResourceSummary()
                        },
                        Population = 10
                    },
                    Config = new CityConfigTestData() {
                        UnemployedYield = new ResourceSummary(production: 1f)
                    }
                }).SetName("Adds unemployed yield based on city population and occupied slot count").Returns(
                    new ResourceSummary(production: 5f, science: 10)
                );

                yield return new TestCaseData(new CityResourceTestData() {
                    City = new CityTestData() {
                        Location = new CellTestData() {
                            Slot = new SlotTestData() { BaseYield = new ResourceSummary() },
                            SuppressSlot = true
                        },
                        Buildings = new List<BuildingTestData>() {
                            new BuildingTestData() {
                                Slots = new List<SlotTestData>() {
                                    new SlotTestData() { IsOccupied = true },
                                    new SlotTestData() { IsOccupied = true },
                                }
                            },
                            new BuildingTestData() {
                                Slots = new List<SlotTestData>() {
                                    new SlotTestData() { IsOccupied = true },
                                    new SlotTestData() { IsOccupied = false },
                                }
                            }
                        },
                        CityYieldMultipliers = new ResourceSummary(production: 1),
                        NonLocationTerritory = new List<CellTestData>() {
                            new CellTestData() { Slot = new SlotTestData() { IsOccupied = true } },
                            new CellTestData() { Slot = new SlotTestData() { IsOccupied = true } },
                            new CellTestData() { Slot = new SlotTestData() { IsOccupied = false } }
                        },
                        OwningCivilization = new CivilizationTestData() {
                            YieldMultipliers = new ResourceSummary(production: 0.5f)
                        },
                        Population = 10
                    },
                    Config = new CityConfigTestData() {
                        UnemployedYield = new ResourceSummary(production: 1f)
                    }
                }).SetName("Unemployed yield considers city and civilization multipliers").Returns(
                    new ResourceSummary(production: 5 * 2.5f, science: 10)
                );

                yield return new TestCaseData(new CityResourceTestData() {
                    City = new CityTestData() {
                        Location = new CellTestData() {
                            Slot = new SlotTestData() { BaseYield = new ResourceSummary() },
                            SuppressSlot = true
                        },
                        Buildings = new List<BuildingTestData>() { },
                        CityYieldMultipliers = new ResourceSummary(),
                        NonLocationTerritory = new List<CellTestData>() { },
                        OwningCivilization = new CivilizationTestData() {
                            YieldMultipliers = new ResourceSummary()
                        },
                        Population = 10
                    },
                    Config = new CityConfigTestData() {
                        UnemployedYield = new ResourceSummary()
                    }
                }).SetName("Generates 1 science per population").Returns(
                    new ResourceSummary(science: 10)
                );

                yield return new TestCaseData(new CityResourceTestData() {
                    City = new CityTestData() {
                        Location = new CellTestData() {
                            Slot = new SlotTestData() { BaseYield = new ResourceSummary() },
                            SuppressSlot = true
                        },
                        Buildings = new List<BuildingTestData>() { },
                        CityYieldMultipliers = new ResourceSummary(science: 1.5f),
                        NonLocationTerritory = new List<CellTestData>() { },
                        OwningCivilization = new CivilizationTestData() {
                            YieldMultipliers = new ResourceSummary(science: -2f)
                        },
                        Population = 10
                    },
                    Config = new CityConfigTestData() {
                        UnemployedYield = new ResourceSummary()
                    }
                }).SetName("Science per population considers city and civilization multipliers").Returns(
                    new ResourceSummary(science: 5)
                );
            }
        }

        public static IEnumerable GetYieldOfSlotForCityTestCases {
            get {
                yield return new TestCaseData(new SlotResourceTestData() {
                    City = new CityTestData() {
                        Buildings            = new List<BuildingTestData>() { },
                        OwningCivilization   = new CivilizationTestData() { },
                        CityYieldMultipliers = new ResourceSummary(),
                        Location             = new CellTestData() { Slot = new SlotTestData() { } },
                        NonLocationTerritory = new List<CellTestData>() { },
                        Population           = 0
                    },
                    Slot = new SlotTestData() {
                        BaseYield = new ResourceSummary(food: 1f, production: 2f, gold: 3f),
                        IsOccupied = true,
                        YieldMultipliers = ResourceSummary.Empty
                    }
                }).SetName("Occupied slot, no additional modifiers").Returns(new ResourceSummary(
                    food: 1f, production: 2f, gold: 3f
                ));

                yield return new TestCaseData(new SlotResourceTestData() {
                    City = new CityTestData() {
                        Buildings            = new List<BuildingTestData>() { },
                        OwningCivilization   = new CivilizationTestData() { },
                        CityYieldMultipliers = new ResourceSummary(),
                        Location             = new CellTestData() { Slot = new SlotTestData() { } },
                        NonLocationTerritory = new List<CellTestData>() { },
                        Population           = 0
                    },
                    Slot = new SlotTestData() {
                        BaseYield = new ResourceSummary(food: 1f, production: 2f, gold: 3f),
                        IsOccupied = false,
                        YieldMultipliers = ResourceSummary.Empty
                    }
                }).SetName("Unoccupied slot, no additional modifiers").Returns(new ResourceSummary(
                    food: 1f, production: 2f, gold: 3f
                ));

                yield return new TestCaseData(new SlotResourceTestData() {
                    City = new CityTestData() {
                        Buildings            = new List<BuildingTestData>() { },
                        OwningCivilization   = new CivilizationTestData() { },
                        CityYieldMultipliers = new ResourceSummary(),
                        Location             = new CellTestData() { Slot = new SlotTestData() { } },
                        NonLocationTerritory = new List<CellTestData>() { },
                        Population           = 0
                    },
                    Slot = new SlotTestData() {
                        BaseYield = new ResourceSummary(food: 1f, production: 2f, gold: 3f),
                        IsOccupied = false,
                        YieldMultipliers = new ResourceSummary(food: 0f, production: -0.5f, gold: 2f, culture: 3f)
                    }
                }).SetName("Occupied slot with yield multipliers").Returns(new ResourceSummary(
                    food: 1f, production: 1f, gold: 9f
                ));

                yield return new TestCaseData(new SlotResourceTestData() {
                    City = new CityTestData() {
                        Buildings            = new List<BuildingTestData>() { },
                        OwningCivilization   = new CivilizationTestData() { },
                        CityYieldMultipliers = new ResourceSummary(food: 0f, production: -0.5f, gold: 2f, culture: 3f),
                        Location             = new CellTestData() { Slot = new SlotTestData() { } },
                        NonLocationTerritory = new List<CellTestData>() { },
                        Population           = 0
                    },
                    Slot = new SlotTestData() {
                        BaseYield = new ResourceSummary(food: 1f, production: 2f, gold: 3f),
                        IsOccupied = true,
                        YieldMultipliers = ResourceSummary.Empty
                    }
                }).SetName("Occupied slot, city has yield modifiers").Returns(new ResourceSummary(
                    food: 1f, production: 1f, gold: 9f
                ));

                yield return new TestCaseData(new SlotResourceTestData() {
                    City = new CityTestData() {
                        Buildings            = new List<BuildingTestData>() { },
                        OwningCivilization   = new CivilizationTestData() {
                            YieldMultipliers = new ResourceSummary(food: 0f, production: -0.5f, gold: 2f, culture: 3f)
                        },
                        CityYieldMultipliers = new ResourceSummary(),
                        Location             = new CellTestData() { Slot = new SlotTestData() { } },
                        NonLocationTerritory = new List<CellTestData>() { },
                        Population           = 0
                    },
                    Slot = new SlotTestData() {
                        BaseYield = new ResourceSummary(food: 1f, production: 2f, gold: 3f),
                        IsOccupied = true,
                        YieldMultipliers = ResourceSummary.Empty
                    }
                }).SetName("Occupied slot, civilization has yield modifiers").Returns(new ResourceSummary(
                    food: 1f, production: 1f, gold: 9f
                ));

                yield return new TestCaseData(new SlotResourceTestData() {
                    City = new CityTestData() {
                        Buildings            = new List<BuildingTestData>() { },
                        OwningCivilization   = new CivilizationTestData() {
                            YieldMultipliers = new ResourceSummary(food: 2f, gold: 1f)
                        },
                        CityYieldMultipliers = new ResourceSummary(food: 1, production: 1),
                        Location             = new CellTestData() { Slot = new SlotTestData() { } },
                        NonLocationTerritory = new List<CellTestData>() { },
                        Population           = 0
                    },
                    Slot = new SlotTestData() {
                        BaseYield = new ResourceSummary(food: 1f, production: 1f, gold: 1f, culture: 1f),
                        IsOccupied = true,
                        YieldMultipliers = new ResourceSummary(food: -1f, production: 3f, gold: 0, culture: -5f)
                    }
                }).SetName("Occupied slot, city, civilization, and slot modifiers in play").Returns(new ResourceSummary(
                    food: 3f, production: 5f, gold: 2f, culture: -4f
                ));
            }
        }

        public static IEnumerable GetYieldOfUnemployedForCityTestCases {
            get {
                yield return new TestCaseData(new CityResourceTestData() {
                    City = new CityTestData() {
                        Buildings = new List<BuildingTestData>() { },
                        OwningCivilization = new CivilizationTestData() {
                            YieldMultipliers = new ResourceSummary()
                        },
                        CityYieldMultipliers = new ResourceSummary(),
                        Location = new CellTestData() { Slot = new SlotTestData() { } },
                        NonLocationTerritory = new List<CellTestData>() { },
                        Population = 0
                    },
                    Config = new CityConfigTestData() {
                        UnemployedYield = new ResourceSummary(food: 1f, production: 1f)
                    }
                }).SetName("No modifiers in play returns configured base").Returns(
                    new ResourceSummary(food: 1f, production: 1f)
                );

                yield return new TestCaseData(new CityResourceTestData() {
                    City = new CityTestData() {
                        Buildings = new List<BuildingTestData>() { },
                        OwningCivilization = new CivilizationTestData() {
                            YieldMultipliers = new ResourceSummary()
                        },
                        CityYieldMultipliers = new ResourceSummary(food: 2f, production: 3f, gold: -4f),
                        Location = new CellTestData() { Slot = new SlotTestData() { } },
                        NonLocationTerritory = new List<CellTestData>() { },
                        Population = 0
                    },
                    Config = new CityConfigTestData() {
                        UnemployedYield = new ResourceSummary(food: 1f, production: 1f)
                    }
                }).SetName("city modifiers in play").Returns(
                    new ResourceSummary(food: 3f, production: 4f)
                );

                yield return new TestCaseData(new CityResourceTestData() {
                    City = new CityTestData() {
                        Buildings = new List<BuildingTestData>() { },
                        OwningCivilization = new CivilizationTestData() {
                            YieldMultipliers = new ResourceSummary(food: 2f, production: 3f, gold: -4f)
                        },
                        CityYieldMultipliers = new ResourceSummary(),
                        Location = new CellTestData() { Slot = new SlotTestData() { } },
                        NonLocationTerritory = new List<CellTestData>() { },
                        Population = 0
                    },
                    Config = new CityConfigTestData() {
                        UnemployedYield = new ResourceSummary(food: 1f, production: 1f)
                    }
                }).SetName("Civilization modifiers in play").Returns(
                    new ResourceSummary(food: 3f, production: 4f)
                );

                yield return new TestCaseData(new CityResourceTestData() {
                    City = new CityTestData() {
                        Buildings = new List<BuildingTestData>() { },
                        OwningCivilization = new CivilizationTestData() {
                            YieldMultipliers = new ResourceSummary(food: 2f, production: 3f, gold: -4f)
                        },
                        CityYieldMultipliers = new ResourceSummary(food: -1f, production: 0f, culture: 0.5f),
                        Location = new CellTestData() { Slot = new SlotTestData() { } },
                        NonLocationTerritory = new List<CellTestData>() { },
                        Population = 0
                    },
                    Config = new CityConfigTestData() {
                        UnemployedYield = new ResourceSummary(food: 1f, production: 1f, gold: 1f, culture: 1f)
                    }
                }).SetName("City and civilization modifiers in play").Returns(
                    new ResourceSummary(food: 2f, production: 4f, gold: -3f, culture: 1.5f)
                );
            }
        }

        #endregion

        #region instance fields and properties

        private Mock<ICityConfig>                                   MockConfig;
        private Mock<IPossessionRelationship<ICity, IHexCell>>      MockCellPossessionCanon;
        private Mock<IPossessionRelationship<ICity, IBuilding>>     MockBuildingCanon;
        private Mock<IIncomeModifierLogic>                          MockIncomeModifierLogic;
        private Mock<IPossessionRelationship<ICivilization, ICity>> MockCityPossessionCanon;
        private Mock<IPossessionRelationship<IHexCell, ICity>>      MockCityLocationCanon;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockConfig              = new Mock<ICityConfig>();
            MockCellPossessionCanon = new Mock<IPossessionRelationship<ICity, IHexCell>>();
            MockBuildingCanon       = new Mock<IPossessionRelationship<ICity, IBuilding>>();     
            MockIncomeModifierLogic = new Mock<IIncomeModifierLogic>();   
            MockCityPossessionCanon = new Mock<IPossessionRelationship<ICivilization, ICity>>();
            MockCityLocationCanon   = new Mock<IPossessionRelationship<IHexCell, ICity>>();

            Container.Bind<ICityConfig>                                  ().FromInstance(MockConfig             .Object);
            Container.Bind<IPossessionRelationship<ICity, IHexCell>>     ().FromInstance(MockCellPossessionCanon.Object);
            Container.Bind<IPossessionRelationship<ICity, IBuilding>>    ().FromInstance(MockBuildingCanon      .Object);
            Container.Bind<IIncomeModifierLogic>                         ().FromInstance(MockIncomeModifierLogic.Object);
            Container.Bind<IPossessionRelationship<ICivilization, ICity>>().FromInstance(MockCityPossessionCanon.Object);
            Container.Bind<IPossessionRelationship<IHexCell, ICity>>     ().FromInstance(MockCityLocationCanon  .Object);

            Container.Bind<ResourceGenerationLogic>().AsSingle();
        }

        private void SetupConfig(CityConfigTestData configData) {
            MockConfig.Setup(config => config.UnemployedYield).Returns(configData.UnemployedYield);
        }

        #endregion

        #region test

        [TestCaseSource("GetTotalYieldForCityTestCases")]
        [Test(Description = "")]
        public ResourceSummary GetTotalYieldForCityTests(CityResourceTestData testData) {
            SetupConfig(testData.Config);

            var city = BuildCity(testData.City);

            var resourceLogic = Container.Resolve<ResourceGenerationLogic>();

            return resourceLogic.GetTotalYieldForCity(city);
        }

        [TestCaseSource("GetYieldOfSlotForCityTestCases")]
        [Test(Description = "")]
        public ResourceSummary GetYieldOfSlotForCityTests(SlotResourceTestData testData) {
            var city = BuildCity(testData.City);

            var slot = BuildWorkerSlot(testData.Slot);

            var resourceLogic = Container.Resolve<ResourceGenerationLogic>();

            return resourceLogic.GetYieldOfSlotForCity(slot, city);
        }

        [TestCaseSource("GetYieldOfUnemployedForCityTestCases")]
        [Test(Description = "")]
        public ResourceSummary GetYieldOfUnemployedForCityTests(CityResourceTestData testData) {
            SetupConfig(testData.Config);

            var city = BuildCity(testData.City);

            var resourceLogic = Container.Resolve<ResourceGenerationLogic>();

            return resourceLogic.GetYieldOfUnemployedForCity(city);
        }

        #endregion

        #region utilities

        private ICity BuildCity(CityTestData cityData) {
            var mockCity = new Mock<ICity>();

            mockCity.Setup(city => city.Population).Returns(cityData.Population);

            var newCity = mockCity.Object;

            var location = BuildCell(cityData.Location);

            MockCityLocationCanon
                .Setup(canon => canon.GetOwnerOfPossession(newCity)).Returns(location);

            List<IHexCell> territory = cityData.NonLocationTerritory.Select(cellData => BuildCell(cellData)).ToList();

            territory.Add(location);

            MockCellPossessionCanon
                .Setup(canon => canon.GetPossessionsOfOwner(newCity))
                .Returns(territory);

            MockBuildingCanon
                .Setup(canon => canon.GetPossessionsOfOwner(newCity))
                .Returns(cityData.Buildings.Select(buildingData => BuildBuilding(buildingData)));

            var owningCiv = BuildCivilization(cityData.OwningCivilization);

            MockCityPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(newCity)).Returns(owningCiv);

            MockIncomeModifierLogic
                .Setup(canon => canon.GetYieldMultipliersForCity(newCity))
                .Returns(cityData.CityYieldMultipliers);

            return newCity;
        }

        private IHexCell BuildCell(CellTestData cellData) {
            var mockCell = new Mock<IHexCell>();

            mockCell.Setup(cell => cell.WorkerSlot  ).Returns(BuildWorkerSlot(cellData.Slot));
            mockCell.Setup(cell => cell.SuppressSlot).Returns(cellData.SuppressSlot);

            return mockCell.Object;
        }

        private IBuilding BuildBuilding(BuildingTestData buildingData) {
            var mockBuilding = new Mock<IBuilding>();

            var mockTemplate = new Mock<IBuildingTemplate>();

            mockTemplate.Setup(template => template.BonusYieldPerPopulation).Returns(buildingData.BonusYieldPerPopulation);

            mockBuilding.Setup(building => building.Template).Returns(mockTemplate.Object);

            mockBuilding.Setup(building => building.StaticYield).Returns(buildingData.StaticYield);

            mockBuilding.Setup(building => building.Slots)
                .Returns(buildingData.Slots.Select(slotData => BuildWorkerSlot(slotData)).ToList().AsReadOnly());            

            return mockBuilding.Object;
        }

        private IWorkerSlot BuildWorkerSlot(SlotTestData slotData) {
            var mockSlot = new Mock<IWorkerSlot>();

            mockSlot.Setup(slot => slot.BaseYield ).Returns(slotData.BaseYield);
            mockSlot.Setup(slot => slot.IsOccupied).Returns(slotData.IsOccupied);

            var newSlot = mockSlot.Object;

            MockIncomeModifierLogic
                .Setup(logic => logic.GetYieldMultipliersForSlot(newSlot))
                .Returns(slotData.YieldMultipliers);

            return newSlot;
        }

        private ICivilization BuildCivilization(CivilizationTestData civData) {
            var newCivilization = new Mock<ICivilization>().Object;

            MockIncomeModifierLogic
                .Setup(logic => logic.GetYieldMultipliersForCivilization(newCivilization))
                .Returns(civData.YieldMultipliers);

            return newCivilization;
        }

        #endregion

        #endregion

    }

}
