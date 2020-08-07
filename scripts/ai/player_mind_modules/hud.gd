extends PlayerMindModule

const velocity_direction_marker_scene: PackedScene = preload("res://scenes/ui/velocity_direction_marker.tscn");

var velocity_direction_marker:Control = velocity_direction_marker_scene.instance();

var _master_mind: Node;

var _boid_contact_indicators := {};

const boid_indicator_scene: PackedScene = preload("res://scenes/ui/boid_indicator.tscn");
const aim_leading_scene = preload("res://scenes/ui/target_aim_leading.tscn");

# FIXME: find a sane number for these
const boid_indicator_pool_size := 9999;
const aim_lead_pool_size := 9999;

var _boid_indicator_pool := ObjectPool.new_pool(
	boid_indicator_pool_size, 
	funcref(self, "_generate_boid_indicator"), 
	ObjectPool.Policy.SoftLimited
);

var _aim_indicator_pool := ObjectPool.new_pool(
	aim_lead_pool_size, 
	funcref(self, "_generate_aim_leading"), 
	ObjectPool.Policy.SoftLimited
);


func _generate_boid_indicator() -> Control:
	var indicator := boid_indicator_scene.instance() as BoidIndicator;
	return indicator;


func _generate_aim_leading() -> Control:
	var indicator := aim_leading_scene.instance() as AimLeadIndicator;
	return indicator;


func _craft_changed(craft: CraftMaster) -> void:
	._craft_changed(craft);
	velocity_direction_marker.craft = craft;
	# disable indicators for the newly piloted craft
	# and enable them for the previously piloted
	if active_craft:
		_contact_made(active_craft.presence);
	_contact_lost(craft.presence);
	active_craft = craft;
	for contact in _boid_contact_indicators:
		var indicator := _boid_contact_indicators[contact] as BoidIndicator;
		indicator.player_craft = craft;


func _ready() -> void:
	
	Globals.game_world_hud_layer.add_child(velocity_direction_marker);
	_master_mind = get_tree().get_nodes_in_group("MasterMind")[0];
	assert(OK == _master_mind.connect("ContactMade", self, "_contact_made"));
	assert(OK == _master_mind.connect("ContactLost", self, "_contact_lost"));
	# give everyone chance to be ready
	yield(get_tree(),"idle_frame");
	var active_craft_presence= null;
	if active_craft:
		active_craft_presence = active_craft.presence;
	for contact in _master_mind.MasterContactList:
		if contact == active_craft_presence:
			continue;
		_contact_made(contact);


func _contact_made(contact: Spatial) -> void:
	if !(contact.get_script() == CSharp.Boid):
		return;
	var target := contact.GetBody() as RigidBody;
	var boid_indicator := _boid_indicator_pool.GetObject() as BoidIndicator;
	Globals.game_world_hud_layer.add_child(boid_indicator);
	boid_indicator.initialize(target, active_craft, _aim_indicator_pool);
	_boid_contact_indicators[contact] = boid_indicator;


func _contact_lost(contact: Spatial) -> void:
	var boid_indicator := _boid_contact_indicators.get(contact) as BoidIndicator;
	if !boid_indicator:
		return;
	Globals.game_world_hud_layer.remove_child(boid_indicator);
	boid_indicator.reset();
	_boid_indicator_pool.ReturnObject(boid_indicator);
