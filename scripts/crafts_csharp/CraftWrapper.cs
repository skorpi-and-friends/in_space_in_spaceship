using System;
using Godot;

#if GODOT_Real_IS_DOUBLE
using Real = System.Double;
#else
using Real = System.Single;
#endif
namespace ISIS {
    public struct CraftWrapper {
        public readonly RigidBody CraftActual { get; }
        public readonly CraftStateWrapper State { get; }

        public CraftWrapper(RigidBody craft) {
            CraftActual = craft;
            State = Minds.CraftMind.GetCraftState(craft);
        }

        public CraftWrapper(Minds.CraftMind mind) {
            CraftActual = mind.Craft;
            State = Minds.CraftMind.GetCraftState(CraftActual);
        }
    }
}