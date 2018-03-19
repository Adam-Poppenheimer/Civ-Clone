using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.UI.StateMachine {

    public enum TransitionType {
        //General
        ReturnViaButton,
        ReturnViaClick,
        ToMapEditor,
        ToPlayMode,

        //PlayMode
        ToCitySelected,
        ToUnitSelected,
        ToRangedAttack,
        ToEscapeMenu,
        
        //MapEditor
        TerrainEditing,
        UnitPainting,
        CityPainting,
        CivManaging,
    }

}
