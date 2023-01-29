HEADER
{
	Description = "Sauna Simple Shader";
}

//=========================================================================================================================
// Optional
//=========================================================================================================================
FEATURES
{
    #include "common/features.hlsl"
}

//=========================================================================================================================
// Optional
//=========================================================================================================================
MODES
{
	ToolsVis( S_MODE_TOOLS_VIS );
	Depth( "depth_only.shader" );  
	ToolsShadingComplexity( "tools_shading_complexity.shader" );
	VrForward();
}

//=========================================================================================================================
COMMON
{
	#include "common/shared.hlsl"
}

//=========================================================================================================================

struct VertexInput
{
	#include "common/vertexinput.hlsl"
};

//=========================================================================================================================

struct PixelInput
{
	#include "common/pixelinput.hlsl"
};

//=========================================================================================================================

VS
{
	#include "common/vertex.hlsl"

	//
	// Main
	//
	PixelInput MainVs( INSTANCED_SHADER_PARAMS( VS_INPUT i ) )
	{
		PixelInput o = ProcessVertex( i );

        /*float3 vPositionWs = o.vPositionWs.xyz;
        float4 pPos = Position3WsToPs( vPositionWs.xyz );
		float4 vertex = pPos;
		vertex.xyz = pPos.xyz / pPos.w;
		vertex.x = floor( 240 * vertex.x ) / 240;
		vertex.y = floor( 240 * vertex.y ) / 240;
		vertex.xyz *= pPos.w;

		o.vPositionPs = vertex;*/

		return FinalizeVertex( o );
	}
}

//=========================================================================================================================

PS
{    
    #include "sbox_pixel.fxc"

    #include "common/pixel.config.hlsl"
    #include "common/pixel.material.structs.hlsl"
    #include "common/pixel.lighting.hlsl"
    #include "common/pixel.shading.hlsl"

    #include "common/pixel.material.helpers.hlsl"
    
	CreateInputTexture2D( Color, Srgb, 8, "", "_color", "Material,10/10", Default3( 1.0, 1.0, 1.0 ) );
	CreateTexture2DWithoutSampler( g_tColor ) < Channel( RGB, Box( Color ), Srgb ); OutputFormat( BC7 ); SrgbRead( true ); Filter( POINT ); >;

    CreateInputTexture2D( Normal, Linear, 8, "NormalizeNormals", "_normal", "Material,10/20", Default3( 0.5, 0.5, 1.0 ) );
	CreateTexture2DWithoutSampler( g_tNormal ) < Channel( RGB, Box( Normal ), Linear ); OutputFormat( DXT5 ); SrgbRead( false ); >;


	//
	// Main
	//
	float4 MainPs( PixelInput i ) : SV_Target0
	{
        float2 UV = i.vTextureCoords.xy;

        Material m;
        m.Albedo = Tex2DS( g_tColor, TextureFiltering, UV.xy ).rgb;
        m.Normal = TransformNormal( i, DecodeNormal( Tex2DS( g_tNormal, TextureFiltering, UV.xy ).rgb ) );
        m.Roughness = 1;
        m.Metalness = 0;
        m.AmbientOcclusion = 1;
        m.TintMask = 0;
        m.Opacity = 1;
        m.Emission = 0;
        m.Transmission = 1;

		ShadingModelValveStandard sm;

		return FinalizePixelMaterial( i, m, sm );
	}
}