// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "SLab/RobotTextBoard"
{
	Properties
	{
		_ScanLight("ScanLight", 2D) = "white" {}
		_ScanBox("ScanBox", 2D) = "white" {}
		_MainTex("MainTex", 2D) = "white" {}
		_ScanLine("ScanLine", 2D) = "white" {}
		_LineSpeed("LineSpeed", Range( 0 , 1)) = 1
		[HDR]_EmissionColor("EmissionColor", Color) = (0.6764706,2,1.890467,0)
		_BoxSpeed("BoxSpeed", Range( 0 , 1)) = 1
		_LightSpeed("LightSpeed", Range( 0 , 1)) = 1
		_LineStrength("LineStrength", Range( 0 , 1)) = 1
		_BoxStrength("BoxStrength", Range( 0 , 1)) = 1
		_LightStrength("LightStrength", Range( 0 , 1)) = 1
		_AlphaMask("AlphaMask", 2D) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
        ZTest Always
		Cull Off
		CGINCLUDE
		#include "UnityShaderVariables.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		struct Input
		{
			half2 uv_texcoord;
		};

		uniform half4 _EmissionColor;
		uniform sampler2D _MainTex;
		uniform float4 _MainTex_ST;
		uniform sampler2D _ScanLight;
		uniform half _LightSpeed;
		uniform half _LightStrength;
		uniform sampler2D _ScanLine;
		uniform half _LineSpeed;
		uniform half _LineStrength;
		uniform sampler2D _ScanBox;
		uniform half _BoxSpeed;
		uniform half _BoxStrength;
		uniform sampler2D _AlphaMask;
		uniform float4 _AlphaMask_ST;

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			o.Emission = _EmissionColor.rgb;
			float2 uv_MainTex = i.uv_texcoord * _MainTex_ST.xy + _MainTex_ST.zw;
			float mulTime6 = _Time.y * _LightSpeed;
			float2 appendResult8 = (half2(( i.uv_texcoord.x + mulTime6 ) , i.uv_texcoord.y));
			float mulTime14 = _Time.y * _LineSpeed;
			float2 appendResult16 = (half2(i.uv_texcoord.x , ( i.uv_texcoord.y + mulTime14 )));
			float mulTime23 = _Time.y * _BoxSpeed;
			float2 appendResult26 = (half2(( i.uv_texcoord.x + mulTime23 ) , i.uv_texcoord.y));
			float clampResult31 = clamp( ( ( tex2D( _ScanLight, appendResult8 ).a * _LightStrength ) + ( tex2D( _ScanLine, appendResult16 ).a * _LineStrength ) + ( tex2D( _ScanBox, appendResult26 ).b * _BoxStrength ) ) , 0.0 , 1.0 );
			float2 uv_AlphaMask = i.uv_texcoord * _AlphaMask_ST.xy + _AlphaMask_ST.zw;
			o.Alpha = ( tex2D( _MainTex, uv_MainTex ).a + ( clampResult31 * tex2D( _AlphaMask, uv_AlphaMask ).a ) );
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Unlit alpha:fade keepalpha fullforwardshadows noambient novertexlights nolightmap  nodynlightmap nodirlightmap nofog nometa noforwardadd 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			sampler3D _DitherMaskLOD;
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float3 worldPos : TEXCOORD2;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				o.worldPos = worldPos;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord = IN.customPack1.xy;
				float3 worldPos = IN.worldPos;
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				SurfaceOutput o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutput, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				half alphaRef = tex3D( _DitherMaskLOD, float3( vpos.xy * 0.25, o.Alpha * 0.9375 ) ).a;
				clip( alphaRef - 0.01 );
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=15700
-1680;41;1750;926;1888.202;-51.82224;1;True;False
Node;AmplifyShaderEditor.RangedFloatNode;7;-1733.523,-138.7253;Float;False;Property;_LightSpeed;LightSpeed;7;0;Create;True;0;0;False;0;1;0.271;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;25;-1753.542,399.6314;Float;False;Property;_BoxSpeed;BoxSpeed;6;0;Create;True;0;0;False;0;1;0.313;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;15;-1771.654,141.4272;Float;False;Property;_LineSpeed;LineSpeed;4;0;Create;True;0;0;False;0;1;0.266;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;6;-1426.523,-142.7253;Float;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexCoordVertexDataNode;10;-1463.523,-285.7253;Float;False;0;2;0;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TexCoordVertexDataNode;22;-1438.542,270.6314;Float;False;0;2;0;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleTimeNode;14;-1409.654,139.4272;Float;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexCoordVertexDataNode;12;-1446.654,-3.57276;Float;False;0;2;0;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleTimeNode;23;-1401.542,413.6313;Float;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;11;-1160.523,-106.7253;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;13;-1143.654,175.4272;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;24;-1135.542,449.6313;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;8;-975.5232,-142.7253;Float;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;26;-950.5416,413.6313;Float;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;16;-958.6541,139.4272;Float;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;35;-737.1241,362.9061;Float;False;Property;_LineStrength;LineStrength;8;0;Create;True;0;0;False;0;1;0.228;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;2;-754.8447,168.723;Float;True;Property;_ScanLine;ScanLine;3;0;Create;True;0;0;False;0;de8cf87c49d216645b58197b0a22a0ed;de8cf87c49d216645b58197b0a22a0ed;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;3;-746.6848,-168.493;Float;True;Property;_ScanLight;ScanLight;0;0;Create;True;0;0;False;0;50b8861b012009a4f85cfa33cf2ef805;50b8861b012009a4f85cfa33cf2ef805;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;37;-746.1241,654.9061;Float;False;Property;_BoxStrength;BoxStrength;9;0;Create;True;0;0;False;0;1;0.104;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;5;-762.3077,451.2094;Float;True;Property;_ScanBox;ScanBox;1;0;Create;True;0;0;False;0;403632f5bd195774f8295643a01c8155;403632f5bd195774f8295643a01c8155;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;34;-729.5536,58.12943;Float;False;Property;_LightStrength;LightStrength;10;0;Create;True;0;0;False;0;1;0.271;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;38;-405.1241,473.9061;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;36;-395.1241,177.9061;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;33;-384.3125,15.87326;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;27;-187.7404,96.53159;Float;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;31;-26.97571,95.56573;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;29;-173.1719,417.7144;Float;True;Property;_AlphaMask;AlphaMask;11;0;Create;True;0;0;False;0;None;bd8f508c5b931ca4195a4150c2fe8540;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;1;-319.8899,-171.5077;Float;True;Property;_MainTex;MainTex;2;0;Create;True;0;0;False;0;082f7caede1ea314abf267e96ddaff2b;082f7caede1ea314abf267e96ddaff2b;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;30;152.9911,94.5424;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;32;371.2478,6.772018;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;28;-10.59902,-283.2588;Float;False;Property;_EmissionColor;EmissionColor;5;1;[HDR];Create;True;0;0;False;0;0.6764706,2,1.890467,0;0.01319205,0.5678944,0.8970588,0;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;639.8,-180;Half;False;True;2;Half;ASEMaterialInspector;0;0;Unlit;SLab/RobotTextBoard;False;False;False;False;True;True;True;True;True;True;True;True;False;False;True;False;False;False;False;False;Off;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;0.5;True;True;0;False;Transparent;;Transparent;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;6;0;7;0
WireConnection;14;0;15;0
WireConnection;23;0;25;0
WireConnection;11;0;10;1
WireConnection;11;1;6;0
WireConnection;13;0;12;2
WireConnection;13;1;14;0
WireConnection;24;0;22;1
WireConnection;24;1;23;0
WireConnection;8;0;11;0
WireConnection;8;1;10;2
WireConnection;26;0;24;0
WireConnection;26;1;22;2
WireConnection;16;0;12;1
WireConnection;16;1;13;0
WireConnection;2;1;16;0
WireConnection;3;1;8;0
WireConnection;5;1;26;0
WireConnection;38;0;5;3
WireConnection;38;1;37;0
WireConnection;36;0;2;4
WireConnection;36;1;35;0
WireConnection;33;0;3;4
WireConnection;33;1;34;0
WireConnection;27;0;33;0
WireConnection;27;1;36;0
WireConnection;27;2;38;0
WireConnection;31;0;27;0
WireConnection;30;0;31;0
WireConnection;30;1;29;4
WireConnection;32;0;1;4
WireConnection;32;1;30;0
WireConnection;0;2;28;0
WireConnection;0;9;32;0
ASEEND*/
//CHKSM=B46EA7473D4B7B00F4548805A206C4EC51A70026