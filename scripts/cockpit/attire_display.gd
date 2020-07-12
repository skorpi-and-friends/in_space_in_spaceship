extends CockpitDisplay

class_name CkpitAttire

onready var _omni_progress := find_node("Omni", true) as ProgressBar;
onready var _port_progress := find_node("Port", true) as ProgressBar;
onready var _bow_progress := find_node("Bow", true) as ProgressBar;
onready var _starboard_progress := find_node("Starboard", true) as ProgressBar;
onready var _stern_progress := find_node("Stern", true) as ProgressBar;

func _ready_display() -> void:
	var attire := craft.attires;
	if len(attire.profiles) == 0:
		_omni_progress.value = 0;
		_port_progress.value = 0;
		_bow_progress.value = 0;
		_starboard_progress.value = 0;
		_stern_progress.value = 0;
	var omni_profile_found := false;
	for item in attire.profiles:
		var profile := item as AttireProfile;
		var progress_bar: ProgressBar;
		match profile.coverage:
			AttireProfile.Coverage.OMNI:
				progress_bar = _omni_progress;
				omni_profile_found = true;
			AttireProfile.Coverage.PORT: 
				progress_bar = _port_progress;
			AttireProfile.Coverage.BOW: 
				progress_bar = _bow_progress;
			AttireProfile.Coverage.STARBOARD: 
				progress_bar = _starboard_progress;
			AttireProfile.Coverage.STERN:
				progress_bar = _stern_progress;
		progress_bar.percent_visible = true;
	if !omni_profile_found:
		_omni_progress.visible = false;

func _process(delta: float) -> void:
	if !craft:
		return;
	for item in craft.attires.profiles:
		var profile := item as AttireProfile;
		var progress_bar: ProgressBar;
		match profile.coverage:
			AttireProfile.Coverage.OMNI:
				progress_bar = _omni_progress;
			AttireProfile.Coverage.PORT: 
				progress_bar = _port_progress;
			AttireProfile.Coverage.BOW: 
				progress_bar = _bow_progress;
			AttireProfile.Coverage.STARBOARD: 
				progress_bar = _starboard_progress;
			AttireProfile.Coverage.STERN:
				progress_bar = _stern_progress;
			_:
				printerr("unrecognized attire coverage");
				continue;
		progress_bar.value = profile.remaining_integrityPPH * 100;