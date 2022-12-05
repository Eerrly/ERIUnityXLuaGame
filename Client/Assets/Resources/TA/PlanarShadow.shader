

Shader "DynamicShadow/PlanarShadow" {
	Properties{
	_Intensity("atten",range(1,16))=1

	_PlaneColor("Main Color", Color) = (0, 0, 0, 1)

	_OffsetY("OffsetY",Range(-1, 1)) = 0
	}
	SubShader {
	//pass {      
	//	Tags { "LightMode" = "ForwardBase" }
	//	Material{Diffuse(1,1,1,1)}
	//	Lighting On
	//	}//
	pass {   
		Tags { "LightMode" = "ForwardBase" "RenderType" = "Transparent"  "Queue" = "Transparent"}

		Cull Off
		Stencil
		{
			Ref 0
			Comp equal
			Pass incrWrap
			Fail keep
			ZFail keep
		}
		ZWrite off
		//Cull Front
		//Blend DstColor SrcColor
		Blend Srcalpha OneminusSrcAlpha

		Offset -1,-1
		CGPROGRAM

		//#define FOG_LINEAR

		#pragma vertex vert 
		#pragma fragment frag


    	#pragma multi_compile_fog
		#include "UnityCG.cginc"
		//float4x4 _World2Ground;
		//float4x4 _Ground2World;
		float _Intensity;

		float _OffsetY;

		float4  _PlaneColor;

		struct v2f{
			float4 pos:SV_POSITION;
			float atten:TEXCOORD0;

			UNITY_FOG_COORDS(1)
		};
		v2f vert(float4 vertex: POSITION)
		{
			v2f o;
			float3 litDir;
			litDir = normalize(WorldSpaceLightDir(vertex));  
			//litDir = mul(_World2Ground, float4(litDir, 0)).xyz;
			float4 vt;
			vt = mul(unity_ObjectToWorld, vertex);
			//vt = mul(_World2Ground, vt);
			vt.xz = vt.xz - ((vt.y - _OffsetY) / litDir.y) * litDir.xz;
		
		//vt.x=vt.x-(vt.y/litDir.y)*litDir.x;
		//vt.z=vt.z-(vt.y/litDir.y)*litDir.z;
			vt.y = _OffsetY;
			//vt = mul(_Ground2World, vt);//back to world
			//vt = mul(unity_WorldToObject,vt);
			o.pos = UnityWorldToClipPos(vt);
			o.atten = distance(vertex, vt) / _Intensity;


		UNITY_TRANSFER_FOG(o, o.pos);
		return o;
		}
 		float4 frag(v2f i) : COLOR 
		{
			//return smoothstep(0,1,i.atten/2);
			UNITY_APPLY_FOG(i.fogCoord, _PlaneColor);

			return fixed4(_PlaneColor);
		}
 		ENDCG 
		}//
		//pass {   
		//Tags { "LightMode" = "ForwardAdd" } 
		//Cull Front
		//Blend DstColor SrcColor
		//Offset -2,-1
		//CGPROGRAM
		//#pragma vertex vert 
		//#pragma fragment frag
		//#include "UnityCG.cginc"
		//float4x4 _World2Ground;
		//float4x4 _Ground2World;
		//float _Intensity;
		//struct v2f{
		//	float4 pos:SV_POSITION;
		//	float atten:TEXCOORD0;
		//};
		//v2f vert(float4 vertex:POSITION)
		//{
		//v2f o;
		//float3 litDir;
		//	litDir=normalize(WorldSpaceLightDir(vertex)); 
		//	litDir=mul(_World2Ground,float4(litDir,0)).xyz;
		//float4 vt;
		//	vt= mul(unity_ObjectToWorld, vertex);
		//	vt=mul(_World2Ground,vt);
		//vt.xz=vt.xz-(vt.y/litDir.y)*litDir.xz;
		//vt.y=0;
		//vt=mul(_Ground2World,vt);//back to world
		//vt=mul(unity_WorldToObject,vt);
		//o.pos= UnityObjectToClipPos(vt);
		//o.atten=distance(vertex,vt)/_Intensity;
		//return o;
		//}
 	//	float4 frag(v2f i) : COLOR 
		//{
		//	return smoothstep(0,1,i.atten*i.atten);
		//}
 	//	ENDCG 
		//}
   }
}
