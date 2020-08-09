using System;
using Godot;

public class ScanPresence : Spatial {

    public override void _EnterTree() {
        Name = GetParent().Name; // inherit parent's name
        base._EnterTree();
        AddToGroup("ScanPresence");
    }

    public virtual Node GetPresenceOwner() {
        return GetParent();
    }

}