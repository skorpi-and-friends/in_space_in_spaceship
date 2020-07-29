extends RigidBody

class_name CraftMaster

signal moment_of_inertia_changed(inc_inertia);

export var _config: Resource;

#var powered_on := true;

onready var engine := $Engine as CraftEngine;
onready var arms := $Arms as ArmamentMaster;
onready var attires := $Attire as AttireMaster;

var _moment_of_inertia_inv := Vector3();


func _ready():
	assert(_config is CraftConfig);
	engine._init_from_config(_config, self);
	attires.connect("damage_recieved", self, "recieved_damage");
	if arms.primary_weapon:
		arms.primary_weapon.connect("damage_done", self, "did_damage");
	if arms.secondary_weapon:
		arms.secondary_weapon.connect("damage_done", self, "did_damage");


func recieved_damage(profile: AttireProfile, weapon: Weapon, damage_recieved: float):
	printerr("%s: recieved damage %s by a %s at %s" % [name, damage_recieved, weapon.name, profile.name]);
	weapon._report_damage(self, damage_recieved);


func did_damage(weapon, node, damage):
	printerr("Did damage %s to a %s using %s" % [damage, node.name, weapon.name]);


func get_craft_rigidbody() -> RigidBody:
	return self;


func _integrate_forces(state: PhysicsDirectBodyState):
	var inv_inertia = state.inverse_inertia;
	# in float lingo: if inv_inertia != _moment_of_inertia_inv
	if inv_inertia != _moment_of_inertia_inv:
		_moment_of_inertia_inv = inv_inertia;
		emit_signal("moment_of_inertia_changed", inv_inertia);
