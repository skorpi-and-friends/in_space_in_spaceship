extends MarginContainer

class_name AttireProfileDisplay

onready var _shield_bar := $Shield as ProgressBar;
onready var _armour_bar := $Armour as ProgressBar;
onready var _hull_bar := $Hull as ProgressBar;

var profile: AttireProfile


func _ready() -> void:
	_shield_bar.value = 0;
	_armour_bar.value = 0;
	_hull_bar.value = 0;


func _process(_delta: float) -> void:
	if !profile:
		return;
	_shield_bar.percent_visible = false;
	_armour_bar.percent_visible = false;
	_hull_bar.percent_visible = false;
	var attire_percent_to_display := _hull_bar;
	for item in profile.members:
		var attire := item as Attire;
		var progress_bar: ProgressBar;
		match attire.type:
			Attire.Type.HULL:
				progress_bar = _hull_bar;
			Attire.Type.ARMOUR:
				progress_bar = _armour_bar;
				if attire.remaining_integrityPPH > 0.009:
					attire_percent_to_display = _armour_bar;
			Attire.Type.SHIELD:
				progress_bar = _shield_bar;
				if attire.remaining_integrityPPH > 0.009:
					attire_percent_to_display = _shield_bar;
		progress_bar.value = attire.remaining_integrityPPH * 100;
	attire_percent_to_display.percent_visible = true;
