extends RichTextLabel

func _ready() -> void:
	install_effects()

func install_effects() -> void:
	install_effect(RichTextJitter.new())