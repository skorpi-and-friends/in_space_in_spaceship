extends Spatial

class_name CockpitMaster

onready var _displays:Array = [
	$"Main Display/Viewport/CockpitMainDisplay"
];

func set_craft(craft: CraftMaster):
	for display in _displays:
		var ckpit_display := display as CockpitDisplay;
		assert(ckpit_display);
		ckpit_display.craft = craft;

# TODO: disable cockpit method