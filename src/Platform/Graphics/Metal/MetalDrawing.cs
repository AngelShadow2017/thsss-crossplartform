#nullable enable
using System;
using static THSSS.Graphics.Metal.MetalNative;
namespace THSSS.Graphics.Metal;

public sealed unsafe partial class MetalRenderBackend
{
    private nint _presentLibrary,_presentPipeline;
    private nint BeginPass(MetalFramebuffer frame,ClearMask mask,ClearColor color,float depth,int stencil,out nint command)
    {
        command=Require(Call(_queue,"commandBuffer"),"commandBuffer");
        nint descriptor=Call(objc_getClass("MTLRenderPassDescriptor"),"renderPassDescriptor");
        if(frame.Color is MetalTexture texture)
        {
            nint a=Call(Call(descriptor,"colorAttachments"),"objectAtIndexedSubscript:",0);
            Call(a,"setTexture:",texture.Native);Call(a,"setLoadAction:",(mask&ClearMask.Color)!=0?2:1);Call(a,"setStoreAction:",1);Color(a,color);
        }
        if(frame.Depth!=null)
        {
            nint a=Call(descriptor,"depthAttachment");Call(a,"setTexture:",frame.Depth.Texture.Native);
            Call(a,"setLoadAction:",(mask&ClearMask.Depth)!=0?2:1);Call(a,"setStoreAction:",1);Double(a,"setClearDepth:",depth);
            if(frame.Depth.Texture.PixelFormat==260)
            {a=Call(descriptor,"stencilAttachment");Call(a,"setTexture:",frame.Depth.Texture.Native);Call(a,"setLoadAction:",(mask&ClearMask.Stencil)!=0?2:1);Call(a,"setStoreAction:",1);Call(a,"setClearStencil:",stencil);}
        }
        return Require(Call(command,"renderCommandEncoderWithDescriptor:",descriptor),"render encoder");
    }
    private void Complete(nint command)
    {
        Call(command,"commit");Call(command,"waitUntilCompleted");TotalSubmissions++;
        if(Call(command,"status")==5)throw new InvalidOperationException("Metal command failed: "+Error(Call(command,"error")));
    }
    public void Clear(ClearMask mask,float red,float green,float blue,float alpha,float depth,int stencil)
    {CheckAlive();nint e=BeginPass(DrawTarget,mask,new(red,green,blue,alpha),depth,stencil,out nint c);Call(e,"endEncoding");Complete(c);}
    public void Draw(IRenderShader shader,IRenderBuffer vertices,VertexLayout layout,PrimitiveKind kind,int startVertex,int vertexCount)
    {
        CheckAlive();if(startVertex<0||vertexCount<0||layout.Stride<=0)throw new ArgumentOutOfRangeException(nameof(vertexCount));if(vertexCount==0)return;
        var v=Resource<MetalBuffer>(vertices);var s=Resource<MetalShader>(shader);
        if(((long)startVertex+vertexCount)*layout.Stride>v.SizeInBytes)throw new ArgumentException("Metal vertex draw exceeds buffer.");
        var f=DrawTarget;var t=_texture??(_fallback??=(MetalTexture)CreateTexture(1,1,TextureFormat.Rgba8,new byte[]{255,255,255,255}));
        if(ReferenceEquals(t,f.Color))throw new InvalidOperationException("Cannot sample active Metal attachment.");
        nint pipeline=s.Pipeline(layout,_blend,((MetalTexture?)f.Color)?.PixelFormat??0,f.Depth?.Texture.PixelFormat??0);
        nint e=BeginPass(f,0,default,1,0,out nint c);Call(e,"setRenderPipelineState:",pipeline);SetViewport(e,_viewport);
        int x=_scissorEnabled?Math.Max(0,_scissor.X):0,y=_scissorEnabled?Math.Max(0,_scissor.Y):0;
        int w=_scissorEnabled?Math.Min(_scissor.Width,f.Width-x):f.Width,h=_scissorEnabled?Math.Min(_scissor.Height,f.Height-y):f.Height;
        if(w<=0||h<=0){Call(e,"endEncoding");Complete(c);return;}
        SetScissor(e,new((nuint)x,(nuint)y,(nuint)w,(nuint)h));
        Call(e,"setCullMode:",_cull==CullMode.None?0:_cull==CullMode.Clockwise?1:2);Call(e,"setFrontFacingWinding:",1);
        if(!_depthStates.TryGetValue((_depthTest,_depthWrite),out nint state))
        {
            nint d=New("MTLDepthStencilDescriptor");
            try{Call(d,"setDepthCompareFunction:",_depthTest?3:7);Call(d,"setDepthWriteEnabled:",_depthTest&&_depthWrite?1:0);state=Require(Call(Device,"newDepthStencilStateWithDescriptor:",d),"depth state");_depthStates.Add((_depthTest,_depthWrite),state);}
            finally{Release(d);}
        }
        Call(e,"setDepthStencilState:",state);Call(e,"setVertexBuffer:offset:atIndex:",v.Native,0,30);
        ReadOnlySpan<float> defaultAttribute = stackalloc float[] { 0f, 0f, 0f, 1f };
        fixed (float* values = defaultAttribute)
            Call(e, "setVertexBytes:length:atIndex:", (nint)values, 4 * sizeof(float), MetalShader.DefaultAttributeBufferIndex);
        foreach(var b in _uniforms)fixed(byte*p=b.Value){Call(e,"setVertexBytes:length:atIndex:",(nint)p,b.Value.Length,b.Key);Call(e,"setFragmentBytes:length:atIndex:",(nint)p,b.Value.Length,b.Key);}
        Call(e,"setFragmentTexture:atIndex:",t.Native,1);Call(e,"setFragmentSamplerState:atIndex:",Sampler(_sampling??t.Sampling),1);
        Call(e,"drawPrimitives:vertexStart:vertexCount:",kind switch{PrimitiveKind.TriangleList=>3,PrimitiveKind.TriangleStrip=>4,PrimitiveKind.LineList=>1,_=>throw new ArgumentOutOfRangeException(nameof(kind))},startVertex,vertexCount);
        Call(e,"endEncoding");Complete(c);
    }
    private nint Sampler(SamplerStateBlock? requested)
    {
        var s=requested??new SamplerStateBlock(3,3,2,2,0,1,0,0,0,0,0,false);if(_samplers.TryGetValue(s,out nint result))return result;
        static int Address(int v)=>v switch{1=>2,2=>3,3=>0,4=>4,_=>throw new NotSupportedException("Unknown Metal sampler address.")};
        nint d=New("MTLSamplerDescriptor");
        try
        {
            Call(d,"setSAddressMode:",Address(s.AddressU));Call(d,"setTAddressMode:",Address(s.AddressV));Call(d,"setMinFilter:",s.MinFilter==1?0:1);Call(d,"setMagFilter:",s.MagFilter==1?0:1);
            Call(d,"setMipFilter:",0);Call(d,"setMaxAnisotropy:",Math.Clamp(s.MaxAnisotropy,1,16));
            result=Require(Call(Device,"newSamplerStateWithDescriptor:",d),"sampler state");_samplers.Add(s,result);return result;
        }
        finally{Release(d);}
    }
    public byte[] ReadPixels(int x,int y,int width,int height)
    {
        CheckAlive();var t=(MetalTexture?)ReadTarget.Color??throw new InvalidOperationException("Readback requires a color attachment.");
        if(x<0||y<0||width<=0||height<=0||(long)x+width>t.Width||(long)y+height>t.Height)throw new ArgumentOutOfRangeException(nameof(width));
        byte[] bytes=new byte[checked(width*height*4)];fixed(byte*p=bytes)Read(t.Native,x,y,width,height,p);
        if(t.Format==TextureFormat.Bgra8)for(int i=0;i<bytes.Length;i+=4)(bytes[i],bytes[i+2])=(bytes[i+2],bytes[i]);return bytes;
    }
    public void Present()
    {
        CheckAlive();if(_layer==0){EndFrame();return;}nint drawable=Call(_layer,"nextDrawable");if(drawable==0){EndFrame();return;}
        if(_presentPipeline==0)CreatePresentPipeline();
        nint c=Require(Call(_queue,"commandBuffer"),"presentation command"),p=Call(objc_getClass("MTLRenderPassDescriptor"),"renderPassDescriptor");
        nint color=Call(Call(p,"colorAttachments"),"objectAtIndexedSubscript:",0);Call(color,"setTexture:",Call(drawable,"texture"));Call(color,"setLoadAction:",2);Call(color,"setStoreAction:",1);Color(color,new(0,0,0,1));
        nint e=Require(Call(c,"renderCommandEncoderWithDescriptor:",p),"present encoder");Call(e,"setRenderPipelineState:",_presentPipeline);
        Call(e,"setFragmentTexture:atIndex:",(_presentSource??(MetalTexture)DefaultFramebuffer.Color!).Native,0);Call(e,"drawPrimitives:vertexStart:vertexCount:",3,0,3);
        Call(e,"endEncoding");Call(c,"presentDrawable:",drawable);Complete(c);PresentedFrames++;EndFrame();
    }
    private void CreatePresentPipeline()
    {
        // Render textures retain the GL-compatible row convention. Metal display top (+Y)
        // must sample the texture's upper logical row (v=1), not flip it a second time.
        const string msl="#include <metal_stdlib>\nusing namespace metal;\nstruct O { float4 p [[position]];float2 uv; };\nvertex O pv(uint i [[vertex_id]]) { float2 p=float2((i<<1)&2,i&2);O o;o.p=float4(p*2-1,0,1);o.uv=p;return o; }\nfragment float4 pf(O i [[stage_in]],texture2d<float> t [[texture(0)]]) { constexpr sampler s(coord::normalized,address::clamp_to_edge,filter::linear);return t.sample(s,i.uv); }";
        nint error=0;_presentLibrary=Require(Call(Device,"newLibraryWithSource:options:error:",Text(msl),0,(nint)(&error)),"present library: "+Error(error));
        nint v=Call(_presentLibrary,"newFunctionWithName:",Text("pv")),f=Call(_presentLibrary,"newFunctionWithName:",Text("pf")),d=New("MTLRenderPipelineDescriptor");
        try{Call(d,"setVertexFunction:",v);Call(d,"setFragmentFunction:",f);Call(Call(Call(d,"colorAttachments"),"objectAtIndexedSubscript:",0),"setPixelFormat:",80);_presentPipeline=Require(Call(Device,"newRenderPipelineStateWithDescriptor:error:",d,(nint)(&error)),"present pipeline: "+Error(error));}
        finally{Release(v);Release(f);Release(d);}
    }
}
