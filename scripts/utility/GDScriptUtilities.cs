using System;
using Godot;

namespace ISIS {
    public class GDScriptUtilities : Godot.Object {
        public static bool IsScanPresence(Godot.Object @object) => @object is ScanPresence;
    }
}