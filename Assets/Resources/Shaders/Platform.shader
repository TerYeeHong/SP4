Shader "Unlit/Platform"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        _innerRadius("innerRadius", Range(0,1)) = 1
        _innerRadiusLength("innerRadiusLength", Range(0,1)) = 1


        _tint("Tint", Color) = (1, 1, 1, 1)
        _tint2("Tint2", Color) = (1, 1, 1, 1)
        _tint3("Tint3", Color) = (1, 1, 1, 1)

        _NoiseScale("Noise Scale", float) = 1
        _NoiseFrequency("Noise Frequency", float) = 1
        _NoiseSpeed("Noise Speed", float) = 1
    }
    SubShader
    {
        Tags {
                        "Queue" = "Transparent"
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "LightMode" = "UniversalForward"

            //"LightMode" = "ShadowCaster"

        }
            //Blend SrcAlpha OneMinusSrcAlpha
            Blend SrcAlpha OneMinusSrcAlpha
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

             #define M_PI 3.1415926535897932384626433832795

                 //
// Noise Shader Library for Unity - https://github.com/keijiro/NoiseShader
//
// Original work (webgl-noise) Copyright (C) 2011 Ashima Arts.
// Translation and modification was made by Keijiro Takahashi.
//
// This shader is based on the webgl-noise GLSL shader. For further details
// of the original shader, please see the following description from the
// original source code.
//

//
// Description : Array and textureless GLSL 2D/3D/4D simplex
//               noise functions.
//      Author : Ian McEwan, Ashima Arts.
//  Maintainer : ijm
//     Lastmod : 20110822 (ijm)
//     License : Copyright (C) 2011 Ashima Arts. All rights reserved.
//               Distributed under the MIT License. See LICENSE file.
//               https://github.com/ashima/webgl-noise
//

            float3 mod289(float3 x)
            {
                return x - floor(x / 289.0) * 289.0;
            }

            float4 mod289(float4 x)
            {
                return x - floor(x / 289.0) * 289.0;
            }

            float4 permute(float4 x)
            {
                return mod289((x * 34.0 + 1.0) * x);
            }

            float4 taylorInvSqrt(float4 r)
            {
                return 1.79284291400159 - r * 0.85373472095314;
            }

            //Simplex noise
            float SimplexNoise(float3 v)
            {
                const float2 C = float2(1.0 / 6.0, 1.0 / 3.0);

                // First corner
                float3 i = floor(v + dot(v, C.yyy));
                float3 x0 = v - i + dot(i, C.xxx);

                // Other corners
                float3 g = step(x0.yzx, x0.xyz);
                float3 l = 1.0 - g;
                float3 i1 = min(g.xyz, l.zxy);
                float3 i2 = max(g.xyz, l.zxy);

                // x1 = x0 - i1  + 1.0 * C.xxx;
                // x2 = x0 - i2  + 2.0 * C.xxx;
                // x3 = x0 - 1.0 + 3.0 * C.xxx;
                float3 x1 = x0 - i1 + C.xxx;
                float3 x2 = x0 - i2 + C.yyy;
                float3 x3 = x0 - 0.5;

                // Permutations
                i = mod289(i); // Avoid truncation effects in permutation
                float4 p =
                    permute(permute(permute(i.z + float4(0.0, i1.z, i2.z, 1.0))
                        + i.y + float4(0.0, i1.y, i2.y, 1.0))
                        + i.x + float4(0.0, i1.x, i2.x, 1.0));

                // Gradients: 7x7 points over a square, mapped onto an octahedron.
                // The ring size 17*17 = 289 is close to a multiple of 49 (49*6 = 294)
                float4 j = p - 49.0 * floor(p / 49.0);  // mod(p,7*7)

                float4 x_ = floor(j / 7.0);
                float4 y_ = floor(j - 7.0 * x_);  // mod(j,N)

                float4 x = (x_ * 2.0 + 0.5) / 7.0 - 1.0;
                float4 y = (y_ * 2.0 + 0.5) / 7.0 - 1.0;

                float4 h = 1.0 - abs(x) - abs(y);

                float4 b0 = float4(x.xy, y.xy);
                float4 b1 = float4(x.zw, y.zw);

                //float4 s0 = float4(lessThan(b0, 0.0)) * 2.0 - 1.0;
                //float4 s1 = float4(lessThan(b1, 0.0)) * 2.0 - 1.0;
                float4 s0 = floor(b0) * 2.0 + 1.0;
                float4 s1 = floor(b1) * 2.0 + 1.0;
                float4 sh = -step(h, 0.0);

                float4 a0 = b0.xzyw + s0.xzyw * sh.xxyy;
                float4 a1 = b1.xzyw + s1.xzyw * sh.zzww;

                float3 g0 = float3(a0.xy, h.x);
                float3 g1 = float3(a0.zw, h.y);
                float3 g2 = float3(a1.xy, h.z);
                float3 g3 = float3(a1.zw, h.w);

                // Normalise gradients
                float4 norm = taylorInvSqrt(float4(dot(g0, g0), dot(g1, g1), dot(g2, g2), dot(g3, g3)));
                g0 *= norm.x;
                g1 *= norm.y;
                g2 *= norm.z;
                g3 *= norm.w;

                // Mix final noise value
                float4 m = max(0.6 - float4(dot(x0, x0), dot(x1, x1), dot(x2, x2), dot(x3, x3)), 0.0);
                m = m * m;
                m = m * m;

                float4 px = float4(dot(x0, g0), dot(x1, g1), dot(x2, g2), dot(x3, g3));
                return 42.0 * dot(m, px);
            }


            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            uniform float _NoiseFrequency, _NoiseScale, _NoiseSpeed;
            uniform float _PixelOffset;

            sampler2D _MainTex;
            float4 _MainTex_ST;

            uniform float4 _tint;
            uniform float4 _tint2;
            uniform float4 _tint3;

            uniform float _innerRadius, _innerRadiusLength;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 center = float2(0.5, 0.5);
                float4 color = _tint;

                //Noise
                float3 spos = float3(i.uv.x, i.uv.y, 0) * _NoiseFrequency;
                spos.z += _Time.x * _NoiseSpeed;
                float noise = _NoiseScale * ((SimplexNoise(spos) + 1) / 2);

                float4 noiseColor = float4(noise, noise, noise, 1);

                //FIRST QUAD
                //first quad
                if ((i.uv.x < center.x + _innerRadius + _innerRadiusLength)
                    && (i.uv.x > center.x - _innerRadius - _innerRadiusLength)
                    && (i.uv.y < center.y + _innerRadius + _innerRadiusLength)
                    && (i.uv.y > center.y - _innerRadius - _innerRadiusLength)) {

                    //Second quad
                    if ((i.uv.x < center.x + _innerRadius)
                        && (i.uv.x > center.x - _innerRadius)
                        && (i.uv.y < center.y + _innerRadius)
                        && (i.uv.y > center.y - _innerRadius)) {
                        
                        //draw a flower :(
                        if (i.uv.x > center.x - (sin(i.uv.y * M_PI)) * 0.2
                            && i.uv.x < center.x + 0.03 - (sin(i.uv.y * M_PI)) * 0.2
                            ) {
                            return float4(1, 1, 1, 1);
                        }
                        if (i.uv.x < center.x + (sin(i.uv.y * M_PI)) * 0.2
                            && i.uv.x > center.x - 0.03 + (sin(i.uv.y * M_PI)) * 0.2
                            ) {
                            return float4(1, 1, 1, 1);
                        }
                        if (i.uv.y > center.y - (sin(i.uv.x * M_PI)) * 0.2
                            && i.uv.y < center.y + 0.03 - (sin(i.uv.x * M_PI)) * 0.2
                            ) {
                            return float4(1, 1, 1, 1);
                        }
                        if (i.uv.y < center.y + (sin(i.uv.x * M_PI)) * 0.2
                            && i.uv.y > center.y - 0.03 + (sin(i.uv.x * M_PI)) * 0.2
                            ) {
                            return float4(1, 1, 1, 1);
                        }
                        ////CIRCLE
                        //if (i.uv.x > center.x - sin(i.uv.y * i.uv.x)
                        //   && i.uv.x < center.x + 0.1 - sin(i.uv.y * i.uv.x)
                        //    //&& i.uv. center.x + sin(i.uv.y * i.uv.x)
                        //    //&& i.uv.y < center.x + sin(i.uv.x)
                        //    ) {
                        //    return float4(1, 1, 1, 1);
                        //}
                        ////CIRCLE
                        //if (i.uv.y > center.y - sin(i.uv.y * i.uv.x)
                        //    && i.uv.y < center.y + 0.1 - sin(i.uv.y * i.uv.x)
                        //    //&& i.uv. center.x + sin(i.uv.y * i.uv.x)
                        //    //&& i.uv.y < center.x + sin(i.uv.x)
                        //    ) {
                        //    return float4(1, 1, 1, 1);
                        //}
                        ////CIRCLE
                        //if ( 1 - i.uv.x < center.x + sin(i.uv.y * i.uv.x) * -1
                        //    && i.uv.x > center.x - 0.1 + sin(i.uv.y * i.uv.x) * -1
                        //    //&& i.uv. center.x + sin(i.uv.y * i.uv.x)
                        //    //&& i.uv.y < center.x + sin(i.uv.x)
                        //    ) {
                        //    return float4(1, 1, 1, 1);
                        //}
                        ////CIRCLE
                        //if (i.uv.y > center.y - sin(i.uv.y * i.uv.x)
                        //    && i.uv.y < center.y + 0.1 - sin(i.uv.y * i.uv.x)
                        //    //&& i.uv. center.x + sin(i.uv.y * i.uv.x)
                        //    //&& i.uv.y < center.x + sin(i.uv.x)
                        //    ) {
                        //    return float4(1, 1, 1, 1);
                        //}
                        ////CIRCLE
                        //if (i.uv.x > center.x - sin(i.uv.y * i.uv.x) * -1
                        //   && i.uv.x < center.x + 0.1 - sin(i.uv.y * i.uv.x) * -1
                        //    //&& i.uv. center.x + sin(i.uv.y * i.uv.x)
                        //    //&& i.uv.y < center.x + sin(i.uv.x)
                        //    ) {
                        //    return float4(1, 1, 1, 1);
                        //}
                        ////CIRCLE
                        //if (i.uv.y > center.y - sin(i.uv.y * i.uv.x) * -1
                        //    && i.uv.y < center.y + 0.1 - sin(i.uv.y * i.uv.x) * -1
                        //    //&& i.uv. center.x + sin(i.uv.y * i.uv.x)
                        //    //&& i.uv.y < center.x + sin(i.uv.x)
                        //    ) {
                        //    return float4(1, 1, 1, 1);
                        //}

                        return noiseColor * _tint;
                        //return _tint;
                    }
                    return _tint2 * noiseColor * 3;
                }
                return _tint3 * noiseColor * 3;
            }
            ENDHLSL
        }
    }
}
