extends CraftMaster

class_name ChildCraftMaster

var mother: ChildCraftMaster;

var is_attached:= false;

func was_attached() -> void:
	is_attached = true;
	mode = RigidBody.MODE_STATIC;
	call_deferred("remove_child", engine);
	call_deferred("remove_child", attires);
	call_deferred("remove_child", arms);


func was_detached() -> void:
	is_attached = false;
	mode = RigidBody.MODE_RIGID;
	add_child(engine);
	add_child(attires);
	add_child(arms);
