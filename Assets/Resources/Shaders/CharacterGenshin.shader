
Shader "Custom/CharacterGenshin"
{
	Properties{
		//_mainTexture("Texture", 2D) = "white" {}
		//_tint("Tint", Color) = (1, 1, 1, 1)
		_diffuse("Diffuse", Range(0,1)) = 1
		_alphaCutoff("Alpha Cutoff", Range(0,1)) = 0.5

		_smoothness("Smoothness", Range(0,1)) = 0.5
		[Gamma]_metallic("Metallic", Range(0,1)) = 0

		//Outline
		_outlineSize("Outline Size", Range(0,1)) = 0.2
		_outlineColour("Outline Colour", Color) = (0,0,0,1)

		//Toon
		_Shades("Shades", Range(0,20)) = 3
		_MinimumShadeDiffuse("_MinimumShadeDiffuse", Range(0,100)) = 1


		//Genshin
		_lightSmooth("_LightSmooth", Range(0,10)) = 0.1 //0.1 for soft shadows
		_tint("Tint", Color) = (1, 1, 1, 1)
		_tintShadow("ShadowTint", Color) = (1, 1, 1, 1)
		_mainTexture("Albedo", 2D) = "red"

		_shadowTex("Shadow Texture", 2D) = "white" {}
		_useShadow("UseShadow", Range(0,1)) = 1


	}

	//Sub shaders are used for different build platforms or level of details
	//eg. one for mobile, one for desktops, etc
	SubShader{
		//Cull Off ZWrite Off ZTest Always
		ZWrite On ZTest Less

		Tags {
			"Queue" = "Transparent"
			//"Queue" = "Opaque"
            //"RenderType" = "Opaque"
            "RenderType" = "Transparent"
			"RenderPipeline" = "UniversalPipeline"
			//"LightMode" = "ForwardBase"
			"LightMode" = "UniversalForward"

			//"LightMode" = "ShadowCaster"
			
		}
		//Blend SrcAlpha OneMinusSrcAlpha
		Blend SrcAlpha OneMinusSrcAlpha

        //            Name "ForwardLit"
        //Tags{"LightMode" = "UniversalForward"}

        //Tags{"RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "UniversalMaterialType" = "Lit" "IgnoreProjector" = "True" "ShaderModel" = "4.5"}
        //LOD 300



		//A shader pass is where an object gets rendered
		//By default, one pass is always needed,
		//no. of pass determines how many times object gets rendered
		Pass {

			//Unity's shaidng language starts here (the real shader starts here)
			HLSLPROGRAM

			//CGPROGRAM instead of HLSLPROGRAM will automatically include UnityCG.cginc
			#include "UnityCG.cginc"

			#include "UnityLightingCommon.cginc"
			//#include "UnityStandardUtils.cginc"

			//#include "UnityPBSLighting.cginc"

			#pragma vertex MyVertexShader
			#pragma fragment MyFragmentShader

			#pragma target 3.0	

			float _lightSmooth;
			float4 _tint;
			float4 _tintShadow;
			sampler2D _mainTexture;

			//IT IS IMPORTANT TO ADD IN _ST, 
			//althought it refers to Scale and Translation due to Unity's backwards compatibility
			//however, It is important for tiling and offset
			float4 _mainTexture_ST;
			sampler2D _shadowTex;
			uint _useShadow;

			uniform float _alphaCutoff;
			uniform float _smoothness;
			uniform float _metallic;

			float _diffuse;
			float _outlineSize;
			float4 _outlineColour;
			float _Shades;
			float _MinimumShadeDiffuse;

			float _shadowBias = 0.1;

			//Character variables to get from script
			float3 _HeadForward;
			float3 _HeadRight;




			//temp
			float shadowDirAngle;

			struct vertexData {
				float4 position : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
			};

			struct vertex2Fragment {
				float2 uv : TEXCOORD0;
				float4 position: SV_POSITION;
				float3 normal : NORMAL;
				float3 worldPosition : POSITION1;
			};

			struct fragmentVertex {
				float2 uv : TEXCOORD0;
				float4 position: SV_POSITION;
			};



			vertex2Fragment MyVertexShader(vertexData vd) {
				vertex2Fragment v2f;
				v2f.position = UnityObjectToClipPos(vd.position);
				v2f.worldPosition = mul(unity_ObjectToWorld, vd.position);
				v2f.uv = TRANSFORM_TEX(vd.uv, _mainTexture);

				//we can use a function that help us do that part, it uses explicit matrix mulitplication
				//so it will be faster than tranpose code
				v2f.normal = UnityObjectToWorldNormal(vd.normal);
				v2f.normal = normalize(v2f.normal);

				return v2f;
			}

			float ShadowCalculation(float4 fragPosLightSpace) {
				float3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w;

				projCoords = projCoords * 0.5 + 0.5;
				//float closestDepth = tex2D(_shadowTex, projCoords.xy).r;
				float currentDepth = projCoords.z;
				float shadow = currentDepth - _shadowBias < shadowDirAngle ? 1.0 : 0.0;

				if (projCoords.z > 1.0 && projCoords.z < 0.0)
					shadow = 0.0;
				return shadow;
			}

			float4 MyFragmentShader(vertex2Fragment v2f) : SV_TARGET{
				float3 viewDir = normalize(_WorldSpaceCameraPos - v2f.worldPosition);
				float3 lightDir = _WorldSpaceLightPos0.xyz;
				float3 lightColor = _LightColor0.rgb;
				//float3 lightColor = _MainLightColor.rgb;
				float3 halfVector = normalize(lightDir + viewDir);
				//float3 albedo = tex2D(_mainTexture, v2f.uv).rgb * tint;

				float3 specularTint;
				float oneMinusReflectivity;



				//Find the genshin's light/shadow direction not using normals but by using textures
				//lightDir = normalize(lightDir);
				float dotF = dot(_HeadForward.xz, lightDir.xz);
				float dotR = dot(_HeadRight.xz, lightDir.xz);

				//step also known as a staircase function making the colour nice n sharp
				float dotFStep = step(0, dotF); //checks degree, =0 when 0-90 && 270-360, else =1
				//dotFStep = 0;
				//dotFStep = step(dotF, 0);

				float3 shadowColor = tex2D(_shadowTex, v2f.uv).rgb;

				const float PI = 3.14159265f;
				//Get the angle in degrees
				float dotRAcos = (acos(dotR) / PI) * 2;
				//<0: 90-270;  >0: 0-90, 270-360;
				float dotRAcosDir = (dotR < 0) ? 1 - dotRAcos : dotRAcos - 1;
				float texShadowDir = (dotR < 0) ? shadowColor.g : shadowColor.r;
				//Channel R is for angle 0-180
				//Channel G is for angle 180-360

				//With this we are modifying the shape of the shadow to be based on texture instead of normals
				shadowDirAngle = step(dotRAcosDir, texShadowDir) * dotFStep; //either returns 0 or not 1
						

				//Toon effect to create layers of shades
				float3 normal_direction = v2f.normal;
				normal_direction.y = 0;
				normal_direction = normalize(normal_direction);

				float cosineAngle = dot(v2f.normal, normalize(_WorldSpaceLightPos0.xyz));
					cosineAngle = dot(normal_direction, normalize(_WorldSpaceLightPos0.xyz));
				//float cosineAngle = dot(v2f.normal, normalize(shadowDir * -1));
						
				//Customlighting, Genshin shadows are soft shadows, not hard
				float smooth = smoothstep(0, _lightSmooth, cosineAngle);
				float3 lerp_color = lerp(_tintShadow, _tint, smooth);

				//Contine toon effect for layers of shades
				cosineAngle = max(cosineAngle, 0.0f);
				cosineAngle = (floor(cosineAngle * _Shades + _MinimumShadeDiffuse) / _Shades);


				//Albedo
				float3 albedo;
				albedo = tex2D(_mainTexture, v2f.uv).rgb * lerp_color;
				//albedo = tex2D(_mainTexture, v2f.uv).rgb;
				//albedo = albedo * shadowDir;
				albedo = albedo * cosineAngle; //Apply toon shade

				//albedo = mul(albedo, texShadowDir);
				//Shader outline
				//If normal is perpendicular to camera view dir, Outline is
				float dot_product = dot(viewDir, v2f.normal);
				if (dot_product > -_outlineSize && dot_product < _outlineSize) //
				{
					albedo = _outlineColour;
				}


				//float3 specular = specularTint * lightColor * pow(float(saturate(dot(halfVector, v2f.normal))), _smoothness * 100);
				float3 diffuse;
				//diffuse = albedo * lightColor * saturate(dot(lightDir, v2f.normal));
				//diffuse = albedo * lightColor * saturate(dot(lightDir, normal_direction));
				diffuse = albedo * 0.1;
				//shadowColor = shadowColor * cosineAngle;
				//diffuse = shadowColor * 0.1;
				//texShadowDir
				//diffuse = albedo * lightColor * shadowDirAngle;
				//diffuse = albedo * shadowDirAngle; 
				//	diffuse = albedo * texShadowDir; 

						
				float4 result = float4(diffuse , 1);

				return result;

			}
				ENDHLSL

		}


 //       Pass
 //       {
 //           Name "ShadowCaster"
 //               Tags{ "LightMode" = "ShadowCaster" }

 //               ZWrite On
 //               ZTest LEqual
 //               ColorMask 0
 //               Cull[_Cull]

 //               HLSLPROGRAM
 //               #pragma exclude_renderers gles gles3 glcore
 //               #pragma target 4.5

 //           // -------------------------------------
 //           // Material Keywords
 //           #pragma shader_feature_local_fragment _ALPHATEST_ON
 //           #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

 //           //--------------------------------------
 //           // GPU Instancing
 //           #pragma multi_compile_instancing
 //           #pragma multi_compile _ DOTS_INSTANCING_ON

 //           // -------------------------------------
 //           // Universal Pipeline keywords

 //           // This is used during shadow map generation to differentiate between directional and punctual light shadows, as they use different formulas to apply Normal Bias
 //           #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

 //           #pragma vertex ShadowPassVertex
 //           #pragma fragment ShadowPassFragment

 //           #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
 //           #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
 //           ENDHLSL
 //       }

 //       Pass
 //       {
 //           // Lightmode matches the ShaderPassName set in UniversalRenderPipeline.cs. SRPDefaultUnlit and passes with
 //           // no LightMode tag are also rendered by Universal Render Pipeline
 //           Name "GBuffer"
 //           Tags{"LightMode" = "UniversalGBuffer"}

 //           ZWrite[_ZWrite]
 //           ZTest LEqual
 //           Cull[_Cull]

 //           HLSLPROGRAM
 //           #pragma exclude_renderers gles gles3 glcore
 //           #pragma target 4.5

 //           // -------------------------------------
 //           // Material Keywords
 //           #pragma shader_feature_local _NORMALMAP
 //           #pragma shader_feature_local_fragment _ALPHATEST_ON
 //           //#pragma shader_feature_local_fragment _ALPHAPREMULTIPLY_ON
 //           #pragma shader_feature_local_fragment _EMISSION
 //           #pragma shader_feature_local_fragment _METALLICSPECGLOSSMAP
 //           #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
 //           #pragma shader_feature_local_fragment _OCCLUSIONMAP
 //           #pragma shader_feature_local _PARALLAXMAP
 //           #pragma shader_feature_local _ _DETAIL_MULX2 _DETAIL_SCALED

 //           #pragma shader_feature_local_fragment _SPECULARHIGHLIGHTS_OFF
 //           #pragma shader_feature_local_fragment _ENVIRONMENTREFLECTIONS_OFF
 //           #pragma shader_feature_local_fragment _SPECULAR_SETUP
 //           #pragma shader_feature_local _RECEIVE_SHADOWS_OFF

 //           // -------------------------------------
 //           // Universal Pipeline keywords
 //           #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
 //           //#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
 //           //#pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
 //           #pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
 //           #pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
 //           #pragma multi_compile_fragment _ _SHADOWS_SOFT
 //           #pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
 //           #pragma multi_compile_fragment _ _LIGHT_LAYERS
 //           #pragma multi_compile_fragment _ _RENDER_PASS_ENABLED

 //           // -------------------------------------
 //           // Unity defined keywords
 //           #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
 //           #pragma multi_compile _ SHADOWS_SHADOWMASK
 //           #pragma multi_compile _ DIRLIGHTMAP_COMBINED
 //           #pragma multi_compile _ LIGHTMAP_ON
 //           #pragma multi_compile _ DYNAMICLIGHTMAP_ON
 //           #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT

 //           //--------------------------------------
 //           // GPU Instancing
 //           #pragma multi_compile_instancing
 //           #pragma instancing_options renderinglayer
 //           #pragma multi_compile _ DOTS_INSTANCING_ON

 //           #pragma vertex LitGBufferPassVertex
 //           #pragma fragment LitGBufferPassFragment

 //           #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
 //           #include "Packages/com.unity.render-pipelines.universal/Shaders/LitGBufferPass.hlsl"
 //           ENDHLSL
 //       }

 //           Pass
 //       {
 //           Name "DepthOnly"
 //           Tags{"LightMode" = "DepthOnly"}

 //           ZWrite On
 //           ColorMask 0
 //           Cull[_Cull]

 //           HLSLPROGRAM
 //           #pragma exclude_renderers gles gles3 glcore
 //           #pragma target 4.5

 //           #pragma vertex DepthOnlyVertex
 //           #pragma fragment DepthOnlyFragment

 //           // -------------------------------------
 //           // Material Keywords
 //           #pragma shader_feature_local_fragment _ALPHATEST_ON
 //           #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

 //           //--------------------------------------
 //           // GPU Instancing
 //           #pragma multi_compile_instancing
 //           #pragma multi_compile _ DOTS_INSTANCING_ON

 //           #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
 //           #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
 //           ENDHLSL
 //       }

 //           // This pass is used when drawing to a _CameraNormalsTexture texture
 //           Pass
 //       {
 //           Name "DepthNormals"
 //           Tags{"LightMode" = "DepthNormals"}

 //           ZWrite On
 //           Cull[_Cull]

 //           HLSLPROGRAM
 //           #pragma exclude_renderers gles gles3 glcore
 //           #pragma target 4.5

 //           #pragma vertex DepthNormalsVertex
 //           #pragma fragment DepthNormalsFragment

 //           // -------------------------------------
 //           // Material Keywords
 //           #pragma shader_feature_local _NORMALMAP
 //           #pragma shader_feature_local _PARALLAXMAP
 //           #pragma shader_feature_local _ _DETAIL_MULX2 _DETAIL_SCALED
 //           #pragma shader_feature_local_fragment _ALPHATEST_ON
 //           #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

 //           //--------------------------------------
 //           // GPU Instancing
 //           #pragma multi_compile_instancing
 //           #pragma multi_compile _ DOTS_INSTANCING_ON

 //           #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
 //           #include "Packages/com.unity.render-pipelines.universal/Shaders/LitDepthNormalsPass.hlsl"
 //           ENDHLSL
 //       }

 //           // This pass it not used during regular rendering, only for lightmap baking.
 //           Pass
 //       {
 //           Name "Meta"
 //           Tags{"LightMode" = "Meta"}

 //           Cull Off

 //           HLSLPROGRAM
 //           #pragma exclude_renderers gles gles3 glcore
 //           #pragma target 4.5

 //           #pragma vertex UniversalVertexMeta
 //           #pragma fragment UniversalFragmentMetaLit

 //           #pragma shader_feature EDITOR_VISUALIZATION
 //           #pragma shader_feature_local_fragment _SPECULAR_SETUP
 //           #pragma shader_feature_local_fragment _EMISSION
 //           #pragma shader_feature_local_fragment _METALLICSPECGLOSSMAP
 //           #pragma shader_feature_local_fragment _ALPHATEST_ON
 //           #pragma shader_feature_local_fragment _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
 //           #pragma shader_feature_local _ _DETAIL_MULX2 _DETAIL_SCALED

 //           #pragma shader_feature_local_fragment _SPECGLOSSMAP

 //           #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
 //           #include "Packages/com.unity.render-pipelines.universal/Shaders/LitMetaPass.hlsl"

 //           ENDHLSL
 //       }

 //           Pass
 //       {
 //           Name "Universal2D"
 //           Tags{ "LightMode" = "Universal2D" }

 //           Blend[_SrcBlend][_DstBlend]
 //           ZWrite[_ZWrite]
 //           Cull[_Cull]

 //           HLSLPROGRAM
 //           #pragma exclude_renderers gles gles3 glcore
 //           #pragma target 4.5

 //           #pragma vertex vert
 //           #pragma fragment frag
 //           #pragma shader_feature_local_fragment _ALPHATEST_ON
 //           #pragma shader_feature_local_fragment _ALPHAPREMULTIPLY_ON

 //           #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
 //           #include "Packages/com.unity.render-pipelines.universal/Shaders/Utils/Universal2D.hlsl"
 //           ENDHLSL
 //       }
	//}




 //   SubShader
 //   {
 //       // Universal Pipeline tag is required. If Universal render pipeline is not set in the graphics settings
 //       // this Subshader will fail. One can add a subshader below or fallback to Standard built-in to make this
 //       // material work with both Universal Render Pipeline and Builtin Unity Pipeline
 //       Tags{"RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "UniversalMaterialType" = "Lit" "IgnoreProjector" = "True" "ShaderModel" = "2.0"}
 //       LOD 300

 //       // ------------------------------------------------------------------
 //       //  Forward pass. Shades all light in a single pass. GI + emission + Fog
 //       Pass
 //       {
 //           // Lightmode matches the ShaderPassName set in UniversalRenderPipeline.cs. SRPDefaultUnlit and passes with
 //           // no LightMode tag are also rendered by Universal Render Pipeline
 //           Name "ForwardLit"
 //           Tags{"LightMode" = "UniversalForward"}

 //           Blend[_SrcBlend][_DstBlend]
 //           ZWrite[_ZWrite]
 //           Cull[_Cull]

 //           HLSLPROGRAM
 //           #pragma only_renderers gles gles3 glcore d3d11
 //           #pragma target 2.0

 //       //--------------------------------------
 //       // GPU Instancing
 //       #pragma multi_compile_instancing
 //       #pragma instancing_options renderinglayer

 //       // -------------------------------------
 //       // Material Keywords
 //       #pragma shader_feature_local _NORMALMAP
 //       #pragma shader_feature_local _PARALLAXMAP
 //       #pragma shader_feature_local _RECEIVE_SHADOWS_OFF
 //       #pragma shader_feature_local _ _DETAIL_MULX2 _DETAIL_SCALED
 //       #pragma shader_feature_local_fragment _SURFACE_TYPE_TRANSPARENT
 //       #pragma shader_feature_local_fragment _ALPHATEST_ON
 //       #pragma shader_feature_local_fragment _ALPHAPREMULTIPLY_ON
 //       #pragma shader_feature_local_fragment _EMISSION
 //       #pragma shader_feature_local_fragment _METALLICSPECGLOSSMAP
 //       #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
 //       #pragma shader_feature_local_fragment _OCCLUSIONMAP
 //       #pragma shader_feature_local_fragment _SPECULARHIGHLIGHTS_OFF
 //       #pragma shader_feature_local_fragment _ENVIRONMENTREFLECTIONS_OFF
 //       #pragma shader_feature_local_fragment _SPECULAR_SETUP

 //       // -------------------------------------
 //       // Universal Pipeline keywords
 //       #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
 //       #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
 //       #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
 //       #pragma multi_compile_fragment _ _SHADOWS_SOFT
 //       #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
 //       #pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
 //       #pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
 //       #pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
 //       #pragma multi_compile_fragment _ _LIGHT_LAYERS
 //       #pragma multi_compile_fragment _ _LIGHT_COOKIES
 //       #pragma multi_compile _ _CLUSTERED_RENDERING

 //       // -------------------------------------
 //       // Unity defined keywords
 //       #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
 //       #pragma multi_compile _ SHADOWS_SHADOWMASK
 //       #pragma multi_compile _ DIRLIGHTMAP_COMBINED
 //       #pragma multi_compile _ LIGHTMAP_ON
 //       #pragma multi_compile_fog
 //       #pragma multi_compile_fragment _ DEBUG_DISPLAY

 //       #pragma vertex LitPassVertex
 //       #pragma fragment LitPassFragment

 //       #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
 //       #include "Packages/com.unity.render-pipelines.universal/Shaders/LitForwardPass.hlsl"
 //       ENDHLSL
 //   }

 //   Pass
 //   {
 //       Name "ShadowCaster"
 //       Tags{"LightMode" = "ShadowCaster"}

 //       ZWrite On
 //       ZTest LEqual
 //       ColorMask 0
 //       Cull[_Cull]

 //       HLSLPROGRAM
 //       #pragma only_renderers gles gles3 glcore d3d11
 //       #pragma target 2.0

 //       //--------------------------------------
 //       // GPU Instancing
 //       #pragma multi_compile_instancing

 //       // -------------------------------------
 //       // Material Keywords
 //       #pragma shader_feature_local_fragment _ALPHATEST_ON
 //       #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

 //       // -------------------------------------
 //       // Universal Pipeline keywords

 //       // This is used during shadow map generation to differentiate between directional and punctual light shadows, as they use different formulas to apply Normal Bias
 //       #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

 //       #pragma vertex ShadowPassVertex
 //       #pragma fragment ShadowPassFragment

 //       #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
 //       #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
 //       ENDHLSL
 //   }

 //   Pass
 //   {
 //       Name "DepthOnly"
 //       Tags{"LightMode" = "DepthOnly"}

 //       ZWrite On
 //       ColorMask 0
 //       Cull[_Cull]

 //       HLSLPROGRAM
 //       #pragma only_renderers gles gles3 glcore d3d11
 //       #pragma target 2.0

 //       //--------------------------------------
 //       // GPU Instancing
 //       #pragma multi_compile_instancing

 //       #pragma vertex DepthOnlyVertex
 //       #pragma fragment DepthOnlyFragment

 //       // -------------------------------------
 //       // Material Keywords
 //       #pragma shader_feature_local_fragment _ALPHATEST_ON
 //       #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

 //       #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
 //       #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
 //       ENDHLSL
 //   }

 //       // This pass is used when drawing to a _CameraNormalsTexture texture
 //       Pass
 //       {
 //           Name "DepthNormals"
 //           Tags{"LightMode" = "DepthNormals"}

 //           ZWrite On
 //           Cull[_Cull]

 //           HLSLPROGRAM
 //           #pragma only_renderers gles gles3 glcore d3d11
 //           #pragma target 2.0

 //           #pragma vertex DepthNormalsVertex
 //           #pragma fragment DepthNormalsFragment

 //       // -------------------------------------
 //       // Material Keywords
 //       #pragma shader_feature_local _NORMALMAP
 //       #pragma shader_feature_local _PARALLAXMAP
 //       #pragma shader_feature_local _ _DETAIL_MULX2 _DETAIL_SCALED
 //       #pragma shader_feature_local_fragment _ALPHATEST_ON
 //       #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

 //       //--------------------------------------
 //       // GPU Instancing
 //       #pragma multi_compile_instancing

 //       #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
 //       #include "Packages/com.unity.render-pipelines.universal/Shaders/LitDepthNormalsPass.hlsl"
 //       ENDHLSL
 //   }

 //       // This pass it not used during regular rendering, only for lightmap baking.
 //       Pass
 //       {
 //           Name "Meta"
 //           Tags{"LightMode" = "Meta"}

 //           Cull Off

 //           HLSLPROGRAM
 //           #pragma only_renderers gles gles3 glcore d3d11
 //           #pragma target 2.0

 //           #pragma vertex UniversalVertexMeta
 //           #pragma fragment UniversalFragmentMetaLit

 //           #pragma shader_feature EDITOR_VISUALIZATION
 //           #pragma shader_feature_local_fragment _SPECULAR_SETUP
 //           #pragma shader_feature_local_fragment _EMISSION
 //           #pragma shader_feature_local_fragment _METALLICSPECGLOSSMAP
 //           #pragma shader_feature_local_fragment _ALPHATEST_ON
 //           #pragma shader_feature_local_fragment _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
 //           #pragma shader_feature_local _ _DETAIL_MULX2 _DETAIL_SCALED

 //           #pragma shader_feature_local_fragment _SPECGLOSSMAP

 //           #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
 //           #include "Packages/com.unity.render-pipelines.universal/Shaders/LitMetaPass.hlsl"

 //           ENDHLSL
 //       }
 //       Pass
 //       {
 //           Name "Universal2D"
 //           Tags{ "LightMode" = "Universal2D" }

 //           Blend[_SrcBlend][_DstBlend]
 //           ZWrite[_ZWrite]
 //           Cull[_Cull]

 //           HLSLPROGRAM
 //           #pragma only_renderers gles gles3 glcore d3d11
 //           #pragma target 2.0

 //           #pragma vertex vert
 //           #pragma fragment frag
 //           #pragma shader_feature_local_fragment _ALPHATEST_ON
 //           #pragma shader_feature_local_fragment _ALPHAPREMULTIPLY_ON

 //           #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
 //           #include "Packages/com.unity.render-pipelines.universal/Shaders/Utils/Universal2D.hlsl"
 //           ENDHLSL
 //       }
 //   }
	}
		//FallBack "Legacy Shaders/VertexLit"
	Fallback "VertexLit"
}


