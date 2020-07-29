extends PlayerMindModule

class_name PlayerPackMind

export var crafts := [];
var flagship: CraftMaster;

var _current_craft_index := 0;

func _ready() -> void:
	# wait till next frame 
	# so that all crafts are truly ready
	call_deferred("populate");


func populate() -> void:
	# start the counter at -one
	# so that even a mothership with 0 
	# childrent can be set as the flagship
	var max_sidecraft_ctr := -1;
	for craft in player_mind.get_children():
		if craft is CraftMaster:
			crafts.append(craft);
			if craft is MotherCraftMaster:
				var sidecraft_ctr := 0;
				for side_craft in craft.sire_craft.members:
					sidecraft_ctr+=1;
					# only append if not previously added
					if !crafts.has(side_craft): 
						crafts.append(side_craft);
				
				if sidecraft_ctr > max_sidecraft_ctr:
					flagship = craft;
					max_sidecraft_ctr = sidecraft_ctr;

var all_attached := true;

func _input(event: InputEvent) -> void:
	if event.is_action_pressed("Switch Craft"):
		switch_craft();
	elif event.is_action_pressed("Debug Button"):
		for craft in crafts:
			if craft is MotherCraftMaster:
				for member in craft.sire_craft.members:
					if all_attached:
						craft.sire_craft.detach_craft(member);
					else:
						craft.sire_craft.attach_craft(member);
		all_attached = !all_attached;


func switch_craft():
	while true:
		_current_craft_index = wrapi(
				_current_craft_index + 1, 0, len(crafts));
		var craft := crafts[_current_craft_index] as CraftMaster;
		if craft is ChildCraftMaster:
			if craft.is_attached:
				continue;
		player_mind.take_control_of_craft(craft);
		break;
