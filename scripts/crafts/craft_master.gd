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
	assert(attires.connect("damage_recieved", self, "recieved_damage") == OK);
	if arms.primary_weapon:
		assert(arms.primary_weapon.connect("damage_done", self, "did_damage") == OK);
	if arms.secondary_weapon:
		assert(arms.secondary_weapon.connect("damage_done", self, "did_damage") == OK);


func recieved_damage(profile: AttireProfile, weapon: Weapon, damage_recieved: float):
	if name != "TestFighter2":
		return;
	printerr("%s: recieved damage %s at %s by: %s (%s)" % [name, damage_recieved, profile.name, weapon.owner.name, weapon.name]);
	weapon._report_damage(self, damage_recieved);


func did_damage(weapon, node, damage):
#	printerr("%s: did damage %s to a %s using %s" % [name, damage, node.name, weapon.name]);
	pass;


func get_craft_rigidbody() -> RigidBody:
	return self;


func _integrate_forces(state: PhysicsDirectBodyState):
	var inv_inertia = state.inverse_inertia;
	# in float lingo: if inv_inertia != _moment_of_inertia_inv
	if inv_inertia != _moment_of_inertia_inv:
		_moment_of_inertia_inv = inv_inertia;
		emit_signal("moment_of_inertia_changed", inv_inertia);
