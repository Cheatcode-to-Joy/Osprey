@tool
extends RichTextEffect
class_name RichTextJitter

# Syntax: [jitter freq=12.0 amp=1.0][/jitter]
var bbcode = "jitter"

func _process_custom_fx(char_fx : CharFXTransform) -> bool:
	var freq = char_fx.env.get("freq", 12.0)
	var amp = char_fx.env.get("amp", 1.0)

	var base = fmod(char_fx.elapsed_time + char_fx.relative_index * PI * 1.25, TAU)
	var mod = sin(char_fx.elapsed_time * freq + char_fx.range.x) * 0.33

	char_fx.offset.x = sin(base) * mod * amp
	char_fx.offset.y = cos(base) * mod * amp
	return true