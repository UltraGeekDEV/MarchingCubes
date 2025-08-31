#version 330 core
out vec4 FragColor;
in vec3 vNormal;

const vec3 warm = vec3(1.0,0.6,0.3);
const vec3 cold = vec3(0.3,0.6,1.0);

void main() {
    float light = 0.5*dot(normalize(vNormal),normalize(vec3(2.0,3.0,1.0)))+0.5;
    vec3 col = light * warm + (1.0-light) * cold;
    vec3 norm = vNormal * 0.5 + vec3(0.5,0.5,0.5);
    FragColor = vec4(norm, 1.0);
}