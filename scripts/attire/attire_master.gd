extends Spatial

class_name AttireMaster

signal damage_recieved(profile, weapon, damage)

var profiles := [];

func _ready():
	for child in get_children():
		var profile := child as AttireProfile;
		if !profile: continue;
		assert(profile.connect("contact", self, "contact") == OK);
		profiles.append(profile);
	if profiles.empty():
		printerr("no profiles found on attire master");


func contact(profile: AttireProfile, body):
	if !body.has_method("get_weapon"): return;
	var weapon := body.get_weapon() as Weapon;
	var remaining_damage := profile.damage(weapon.damage, weapon.damage_type);
	emit_signal("damage_recieved", profile, weapon, weapon.damage - remaining_damage);
