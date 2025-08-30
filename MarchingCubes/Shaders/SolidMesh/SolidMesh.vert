#version 330 core
layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec3 aNormal;

uniform mat4 transform;
uniform mat4 projection;
uniform mat4 view;

out vec3 vNormal;

void main() {
    gl_Position = projection * view * transform * vec4(aPosition, 1.0);
    vNormal = mat3(transform) * aNormal;
}