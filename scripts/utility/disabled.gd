extends Spatial

class_name Disabled

# Called when the node enters the scene tree for the first time.
func _enter_tree() -> void:
	for child in get_children():
		remove_child(child);
		child.queue_free();
	queue_free();
