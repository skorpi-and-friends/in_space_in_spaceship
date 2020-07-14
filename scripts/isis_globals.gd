extends Node

const MASS_MODIFIER := 0.001;

# FIXME: global state!
var player_mind: PlayerMind; 
var cockpit_master: CockpitMaster; 
var viewport_master: ViewportMaster;