using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace BalancedThirst.Hud;

public class ExampleOverlayRenderer : IRenderer
{
    MeshRef quadRef;
    ICoreClientAPI capi;
    public IShaderProgram overlayShaderProg;


    public ExampleOverlayRenderer(ICoreClientAPI capi, IShaderProgram overlayShaderProg)
    {
        this.capi = capi;
        this.overlayShaderProg = overlayShaderProg;

        MeshData quadMesh = QuadMeshUtil.GetCustomQuadModelData(-1, -1, 0, 2, 2);
        quadMesh.Rgba = null;

        quadRef = capi.Render.UploadMesh(quadMesh);
    }

    public double RenderOrder
    {
        get { return 1.1; }
    }

    public int RenderRange { get { return 1; } }

    public void Dispose()
    {
        capi.Render.DeleteMesh(quadRef);
        overlayShaderProg.Dispose();
    }

    public void OnRenderFrame(float deltaTime, EnumRenderStage stage)
    {
        IShaderProgram curShader = capi.Render.CurrentActiveShader;
        curShader?.Stop();
        overlayShaderProg?.Use();
        capi.Render.GlToggleBlend(true);
        
        overlayShaderProg?.Uniform("frostVignetting", 10f);
        overlayShaderProg?.BindTexture2D("primaryScene",
            capi.Render.FrameBuffers[(int)EnumFrameBuffer.Primary].ColorTextureIds[0], 0);
        // overlayShaderProg?.Uniform("iTime", capi.World.ElapsedMilliseconds/1000f);

        capi.Render.RenderMesh(quadRef);
        overlayShaderProg?.Stop();
        curShader?.Use();
    }
}