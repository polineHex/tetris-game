Shader "Unlit/S_BlinkUnlit"
{
   Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		
		_GlowColor ("GlowTint", Color) = (1,1,1,1)
		_Glow ("Glow amount", Float) =1
		[HideInInspector] _Blink ("Blink", float) =0
		[HideInInspector] _Render ("Render tile", float) = 0

		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0

		[HideInInspector] _RendererColor("RendererColor", Color) = (1,1,1,1)
		[HideInInspector] _Flip("Flip", Vector) = (1,1,1,1)
		[PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
		[PerRendererData] _EnableExternalAlpha ("Enable Exnternal Alpha", Float) = 0
	}

	SubShader
	{
		Tags
		{ 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}

		Cull Off
		Lighting Off
		ZWrite Off
		Blend One OneMinusSrcAlpha

		Pass
		{
		CGPROGRAM
			#pragma vertex SpriteVert
			#pragma fragment BlinkSpriteFrag
			#pragma target 2.0
			#pragma multi_compile_instancing
			#pragma multi_compile _ PIXELSNAP_ON
			#pragma multi_compile _ ETC1_EXTERNAL_ALPHA
			#include "UnitySprites.cginc"
			
			fixed4 _GlowColor; //new paramters to control
			float _Glow;   //glow to control
			float _Blink; //if we are blinking it is 1
			float _Render; 
			
			
			fixed4 BlinkSpriteFrag(v2f IN) : SV_Target
			{
								
				fixed4 texel = SampleSpriteTexture (IN.texcoord);

				//float showTexel = step(_BlinkLimit, IN.color.a); // if alpha is greater or equal (0.5 in default case) showTexel will be 1 - we show original color, otherwise 0
				//IN.color.a = (IN.color.a - _BlinkLimit)/(1.0 - _BlinkLimit); //remaping alpha to 0-1 Range

				fixed4 c= texel*IN.color*(1-_Blink)+_GlowColor*_Blink*sign(texel.a);
				
				c.rgb *= _Glow;
				c.rgb *= c.a;
				
				return c*_Render;
			}


		ENDCG
		}
	}
}