extends Node

# it seems like the mass limit might be an editor only thing
# will remove this construct after verifying online
# FIXME: something's wrong in the way this const is used
# as chainging it required reconfiguring the PIDs.
const MASS_MODIFIER := 1.0;#0.001;

# these aren't ratios! they're "converters"
const PIXLE2METER := 0.1;
const METER2PIXEL := 1 / PIXLE2METER;

# FIXME: global state!
var player_mind: PlayerMind; 
var cockpit_master: CockpitMaster; 
var viewport_master: ViewportMaster;
var game_world_hud_layer: CanvasLayer;
