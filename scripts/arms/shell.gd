extends Area

class_name Shell

var _active := false;

var _timer := 0.0
var _hit_something := false

var _owner_weapon: Weapon;
var _speed: float;
var _lifetime: float;

func _ready():
	assert(connect("area_entered",self,"on_entered") == OK);
	assert(connect("body_entered",self,"on_entered") == OK);

func _activate(owner: Weapon, speed: float, lifetime: float):
	_owner_weapon = owner;
	_speed = speed;
	_lifetime = lifetime;
	_active = true;


func _physics_process(delta):
	if !_active: return;
	var forward_dir = global_transform.basis.z.normalized();
	global_translate(forward_dir * _speed * delta);
	_timer += delta;
	if _timer >= _lifetime:
		queue_free();


func on_entered(body):
	if !_hit_something && _owner_weapon.has_method("shell_contact"):
		_owner_weapon.shell_contact(self, body);
	_hit_something = true;
	queue_free();


func get_weapon() -> Weapon:
	return _owner_weapon;
