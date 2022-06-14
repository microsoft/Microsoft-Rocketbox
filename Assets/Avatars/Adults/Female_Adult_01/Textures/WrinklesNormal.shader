Shader "Example/Diffuse Bump" {
	Properties{
		_MainTex("Texture", 2D) = "white" {}
	_Blend("TextureBlend", Range(0,1)) = 0.0
	_BumpMap("Bumpmap", 2D) = "bump" {}
	_BumpMap2("Bumpmap", 2D) = "bump" {}


	}
		SubShader{
		Tags{ "RenderType" = "Opaque" }
		CGPROGRAM
#pragma surface surf Lambert
		struct Input {
		float2 uv_MainTex;
		float2 uv_BumpMap;
		float2 uv_BumpMap2;
	};
	half _Blend;
	sampler2D _MainTex;
	sampler2D _BumpMap;
	sampler2D _BumpMap2;

	void surf(Input IN, inout SurfaceOutput o) {
		o.Albedo = tex2D(_MainTex, IN.uv_MainTex).rgb;
		o.Normal = UnpackNormal(lerp(tex2D(_BumpMap, IN.uv_BumpMap), tex2D(_BumpMap2, IN.uv_BumpMap2), _Blend));

	}
	ENDCG
	}
		Fallback "Diffuse"
}