extends BoidIndicator

class_name TargetIndicator

onready var _name_label := find_node("NameLabel") as Label;
onready var _distance_label := find_node("DistanceLabel") as Label;

func _ready() -> void:
	assert(_name_label);
	assert(_distance_label);


func _process(_delta: float) -> void:
	if !target || !player_craft:
		_distance_label.visible = false;
		return;
	
	_distance_label.visible = true;
	var distance = (target.global_transform.origin - player_craft.global_transform.origin).length();
	var format_string: String;
	if distance < 1001:
		format_string = "%0.2f M";
	else:
		distance /= 1000;
		format_string = "%0.2f KM";
		
	_distance_label.text = format_string % distance;


func _set_target(craft: CraftMaster):
	._set_target(craft);
	_name_label.visible = true;
	_name_label.text = craft.name;


func reset() -> void:
	.reset();
	_name_label.visible = false;
	_distance_label.visible = false;
