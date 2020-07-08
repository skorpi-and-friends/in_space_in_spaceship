extends Spatial

class_name ScanPresence


func _enter_tree():
	add_to_group("Presence")


func _exit_tree():
	pass