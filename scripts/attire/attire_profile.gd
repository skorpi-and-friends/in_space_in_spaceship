extends Area

class_name AttireProfile

signal contact(profile, body)

enum Coverage {OMNI, PORT, BOW, STARBOARD, STERN}

export(Coverage) var coverage := Coverage.OMNI;

var members := [];
var colliders := [];

var remaining_integrityPPH:= 0.0;

var remaining_integrity := 0.0;
var factory_integrity := 0.0;
var recovery_rate := 0.0;

func _ready():
	collision_mask = 0;
	assert(connect("area_entered",self,"on_entered") == OK);
	assert(connect("body_entered",self,"on_entered") == OK);
	for child in get_children():
		if child is Attire:
			members.append(child);
		elif child is CollisionShape ||child is CollisionPolygon ||child is CollisionObject:
			colliders.append(child);
	if members.empty():
		printerr("no attires found on attire profile");
	if colliders.empty():
		printerr("no colliders found on attire profile");
	caclulate_integrity_state();

func on_entered(body):
	emit_signal("contact", self, body);

func add_members(new_members: Array):
	members += new_members;

func damage(damage: float, type: int) -> float:
	printerr("Damaged attire %s" % name);
	printerr("Force: %s kN" % (damage / 1000.0));
	var remaining_damage := damage;
	for member in members:
		remaining_damage = member.damage(remaining_damage, type);
		if remaining_damage <= 0: 
			caclulate_integrity_state()
			return 0.0;
	caclulate_integrity_state()
	return remaining_damage;
	
func caclulate_integrity_state():
	remaining_integrityPPH = 0.0;
	remaining_integrity = 0.0;
	factory_integrity = 0.0;
	recovery_rate = 0.0;
	
	var member_count := len(members);
	if member_count == 0: return;
	
	for member in members:
		remaining_integrityPPH += member.remaining_integrityPPH;
		remaining_integrity += member.remaining_integrity;
		factory_integrity += member.factory_integrity;
		recovery_rate += member.recovery_rate;
		
	remaining_integrityPPH /= member_count;
	remaining_integrity /= member_count;
	factory_integrity /= member_count;
	recovery_rate /= member_count;
	
	
