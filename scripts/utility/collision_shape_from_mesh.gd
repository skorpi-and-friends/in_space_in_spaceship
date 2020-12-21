extends CollisionShape

export var mesh_node_path: NodePath;
onready var _mesh_node:= get_node(mesh_node_path) as MeshInstance

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	assert(_mesh_node);
	shape = _mesh_node.mesh.create_convex_shape();
