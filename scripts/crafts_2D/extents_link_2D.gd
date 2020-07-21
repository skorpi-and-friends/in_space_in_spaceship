extends Node

export var _sprite_path: NodePath
export var _engine_path: NodePath

func _ready():
	var sprite = get_node(_sprite_path) as Sprite;
	var engine = get_node(_engine_path) as CraftEngine2D;
	assert(sprite && engine);
	var size = sprite.get_rect().size;
	if size == Vector2():
		printerr("linked mesh has zero volume.");
	engine._craft_extents = size;
	queue_free()
	
