extends Control

class_name BoidIndicator

onready var _position_indicator = $PositionIndicator as PositionIndicator;

var _aim_lead_indicator_pool;

var _aim_leading_indicators := [];

var player_craft: CraftMaster setget _player_craft_changed;
var target: CraftMaster setget _set_target;

func _ready() -> void:
	assert(_position_indicator);

func initialize(set_target: CraftMaster, 
		aim_leading_pool,
		player_piloted_craft: CraftMaster = null) -> void:
	_aim_lead_indicator_pool = aim_leading_pool;
	self.target = set_target;
	if player_piloted_craft:
		self.player_craft = player_piloted_craft;


func _set_target(craft: CraftMaster) -> void:
	target = craft;
	_position_indicator.target = craft;
	for indicator in _aim_leading_indicators:
		indicator.set_deferred("target", craft);


func _player_craft_changed(craft: CraftMaster):
	player_craft = craft;
	_refresh();


func _refresh() -> void:
	for indicator in _aim_leading_indicators:
		indicator.target = null;
		_aim_lead_indicator_pool.ReturnObject(indicator);
		remove_child(indicator);
	_aim_leading_indicators.clear();
	for weapon in player_craft.arms.get_all_weapons():
		var aim_lead_indicator := _aim_lead_indicator_pool.GetObject() as AimLeadIndicator;
		aim_lead_indicator.weapon = weapon;
		aim_lead_indicator.target = target;
		_aim_leading_indicators.append(aim_lead_indicator);
		add_child(aim_lead_indicator);


func reset() -> void:
	target = null;
	_position_indicator.target = null;
	for indicator in _aim_leading_indicators:
		indicator.target = null;
		_aim_lead_indicator_pool.ReturnObject(indicator);
		remove_child(indicator);
	_aim_leading_indicators.clear();


func set_color_scheme(color: Color) -> void:
	_position_indicator.set_color_scheme(color);
