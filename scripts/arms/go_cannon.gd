extends Weapon

onready var shell_scene := preload("res://scenes/arms/shell.tscn");

export var _shell_speed: float;
export var _shell_lifetime: float;
export var _instanitate_offset := Vector3(0, 0, 1);
export var _rounds_per_second := 5.0;

var _time_since_last_round := INF;

func _enter_tree():
	damage_type = Damage.Type.PLASMA;

func _process(delta):
	if !active:
		_time_since_last_round += delta;
		if _time_since_last_round > 1 / _rounds_per_second:
			active = true;

func _activate():
	if !active: return;
	#fire shell
	var clone := shell_scene.instance() as Shell;
#	var scene_root := get_tree().root.get_children()[0] as Node;
	# use the viweport to make sure shell gets instatntiated int
	# the world that holds the gun
	var scene_root := get_viewport();
	scene_root.add_child(clone)
	
	clone.global_transform = self.global_transform.translated(_instanitate_offset);
	clone._activate(self, _shell_speed, _shell_lifetime);
	
	_time_since_last_round = 0.0;
	active = false;


func shell_contact(_shell: Shell, _body: Node):
	pass
