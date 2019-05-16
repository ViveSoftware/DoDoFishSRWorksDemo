//Smooth lerp the tiling animation frame.
Shader "Custom/TileAnimation"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Color("Color", Color) = (1, 1, 1, 1)
        _Column("Cols Count", Int) = 5
        _Row("Rows Count", Int) = 3
        _FrameTime("Per Frame Length", Float) = 0.5
    }

        SubShader
        {
            Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" "PreviewType" = "Plane" }
            Blend One OneMinusSrcAlpha
            ColorMask RGB
            Cull Off Lighting Off ZWrite Off

            LOD 100

            Pass
            {
                CGPROGRAM
                #pragma vertex vert_img
                #pragma fragment frag

                #include "UnityCG.cginc"

                sampler2D _MainTex;
                fixed4 _Color;
                uint _Column;
                uint _Row;
                float _FrameTime;

                fixed4 getFrame(sampler2D tex, float2 uv, float dx, float dy, int frame) 
                {
                    float uX = (uv.x * dx) + fmod(frame, _Column) * dx;
                    float uY = (uv.y * dy) + (frame / _Column) * dy;
                    return tex2D(tex, float2(uX, 1-uY));
                }

                fixed4 frag(v2f_img i) : SV_Target 
                {
                    int totalFrame = _Row * _Column;
                    float frameValue = fmod(_Time.y / _FrameTime, totalFrame);
                    int currentFrame = floor(frameValue);
                    float dx = 1.0 / _Column;
                    float dy = 1.0 / _Row;

                    int nextFrame = floor(fmod(frameValue+1, totalFrame));
                    float currentFrameColor = getFrame(_MainTex, i.uv, dx, dy, currentFrame).a;
                    float nextFrameColor = getFrame(_MainTex, i.uv, dx, dy, nextFrame).a;
                    float frameRatio = frameValue - currentFrame;
                    return lerp(currentFrameColor, nextFrameColor, frameRatio) * _Color;
                }
                ENDCG
            }
        }
}