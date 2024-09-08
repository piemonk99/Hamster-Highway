Shader "SolidColorWithZTest" 
{
    Properties 
    {
        _Color1 ("Color1", Color) = (1,1,0,1)
        _Color2 ("Color2", Color) = (0,1,0,1)
    }
    SubShader 
    {
        Tags { "Queue" = "Geometry+1" }
        Pass 
        { 
            ZTest Greater
            Blend SrcAlpha OneMinusSrcAlpha
            Color [_Color1]
            
        }
        Pass 
        { 
            ZTest Less
            Blend SrcAlpha OneMinusSrcAlpha
            Color [_Color2] 
        }
    }
}