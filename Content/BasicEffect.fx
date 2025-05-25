#if OPENGL
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0_level_9_1
    #define PS_SHADERMODEL ps_4_0_level_9_1
#endif

// Текстура буфера экрана
texture ScreenTexture;
sampler ScreenSampler = sampler_state
{
    Texture = <ScreenTexture>;
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};

// Параметры эффекта
float Brightness = 1.0f;
float Contrast = 1.0f;
float3 AmbientColor = float3(1.0, 1.0, 1.0);
float AmbientIntensity = 0.1f;

// Входные данные для вершинного шейдера
struct VertexShaderInput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
    float4 Color : COLOR0;
};

// Выходные данные вершинного шейдера / входные данные пиксельного шейдера
struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
    float4 Color : COLOR0;
};

// Вершинный шейдер
VertexShaderOutput MainVS(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput)0;
    
    output.Position = input.Position;
    output.TexCoord = input.TexCoord;
    output.Color = input.Color;
    
    return output;
}

// Пиксельный шейдер
float4 MainPS(VertexShaderOutput input) : COLOR0
{
    // Получаем цвет из текстуры
    float4 color = tex2D(ScreenSampler, input.TexCoord) * input.Color;
    
    // Применяем базовые эффекты
    color.rgb = (color.rgb - 0.5f) * Contrast + 0.5f;
    color.rgb *= Brightness;
    
    // Добавляем базовое окружающее освещение
    color.rgb += AmbientColor * AmbientIntensity;
    
    return color;
}

// Техника
technique BasicColorDrawing
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
} 