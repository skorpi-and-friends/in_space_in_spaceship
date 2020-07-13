extends CockpitDisplay

class_name CkpitAttire

onready var _omni_progress := find_node("Omni", true) as AttireProfileDisplay;
onready var _port_progress := find_node("Port", true) as AttireProfileDisplay;
onready var _bow_progress := find_node("Bow", true) as AttireProfileDisplay;
onready var _starboard_progress := find_node("Starboard", true) as AttireProfileDisplay;
onready var _stern_progress := find_node("Stern", true) as AttireProfileDisplay;

func _ready_display() -> void:
	var attire := craft.attires;
	var omni_profile_found := false;
	for item in attire.profiles:
		var profile := item as AttireProfile;
		var display: AttireProfileDisplay;
		match profile.coverage:
			AttireProfile.Coverage.OMNI:
				display = _omni_progress;
				omni_profile_found = true;
			AttireProfile.Coverage.PORT: 
				display = _port_progress;
			AttireProfile.Coverage.BOW:
				display = _bow_progress;
			AttireProfile.Coverage.STARBOARD:
				display = _starboard_progress;
			AttireProfile.Coverage.STERN:
				display = _stern_progress;
		display.profile = profile;
	if !omni_profile_found:
		_omni_progress.visible = false;