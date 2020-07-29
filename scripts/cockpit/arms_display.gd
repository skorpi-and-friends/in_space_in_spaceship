extends CockpitDisplay

class_name CkpitArms

onready var _holo_rack := find_node("HoloRack", true) as VBoxContainer;

func _set_craft() -> void:
	for child in _holo_rack.get_children():
		child.queue_free();
	var arms := craft.arms;
	if arms.primary_weapon:
		_holo_rack.add_child(arms.primary_weapon._get_holo_display());
	if arms.secondary_weapon:
		_holo_rack.add_child(arms.secondary_weapon._get_holo_display());