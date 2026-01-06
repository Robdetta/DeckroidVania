shader_type spatial;
render_mode unshaded, cull_disabled;

uniform float health_ratio : hint_range(0.0, 1.0) = 1.0;
uniform vec4 bar_color : source_color = vec4(0.0, 1.0, 0.0, 1.0);
uniform vec4 background_color : source_color = vec4(0.2, 0.2, 0.2, 1.0);

void vertex() {
    // Billboard: remove rotation, keep position
    MODELVIEW_MATRIX = VIEW_MATRIX * mat4(
        vec4(1,0,0,0),
        vec4(0,1,0,0),
        vec4(0,0,1,0),
        MODEL_MATRIX[3]
    );
}

void fragment() {
    // UV.x goes from 0 → 1 across the plane
    if (UV.x > health_ratio) {
        ALBEDO = background_color.rgb;
    } else {
        ALBEDO = bar_color.rgb;
    }
}