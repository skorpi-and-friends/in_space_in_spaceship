extends RigidBody

class_name CraftMaster

onready var engine := $Engine as CraftEngine;
onready var arms := $Arms as ArmamentMaster;
onready var attires := $Attire as AttireMaster;

func _ready():
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
	
