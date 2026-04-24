extends Node
class_name Proxy, "res://Textures/rpgiab_icon_pack_v1.3/16x16/script_text_delete.png"

# This node at a scene root means that the scene shouldn't be instanced as a normal scene.
# Instead it is instanced inside a constructor of the corresponding node and reparented
# to that node. The proxy node is freed in the process, any inspector values specified in
# it are lost. Rather then setting these values in inspector, they should be set in the
# corresponding node constructor.

func _enter_tree():
	printerr("Proxy should be replaced before entering the scene tree")
