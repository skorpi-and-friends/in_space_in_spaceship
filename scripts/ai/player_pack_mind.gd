extends PlayerMind

class_name PlayerPackMind

export var crafts := [];

var _current_craft_index := 0;

func _ready() -> void:
	._ready();
	for child in get_children():
		var craft := child as CraftMaster;
		if craft:
			crafts.append(craft);
	for craft in crafts:
		for child in craft.get_children():
			var attached_craft := child as CraftMaster;
			if attached_craft:
				crafts.append(attached_craft);


func _input(event: InputEvent) -> void:
	if event.is_action_pressed("Switch Craft"):
		_current_craft_index = wrapi(
				_current_craft_index + 1, 0, len(crafts));
		take_control_of_craft(crafts[_current_craft_index]);