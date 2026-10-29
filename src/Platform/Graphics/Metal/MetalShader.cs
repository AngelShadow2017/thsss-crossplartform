#nullable enable
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using static THSSS.Graphics.Metal.MetalNative;

namespace THSSS.Graphics.Metal;
internal sealed unsafe class MetalShader:MetalResource,IRenderShader
{
    private readonly nint _fragmentLibrary,_vertexFunction,_fragmentFunction;
    private readonly Dictionary<string,nint> _pipelines=new();
    internal MetalShader(MetalRenderBackend owner,ShaderSource source):base(owner,CompileLibrary(owner,source.Vertex,true,source.VertexInputs))
    {
        try
        {
            _fragmentLibrary=CompileLibrary(owner,source.Fragment,false,source.VertexInputs);
            _vertexFunction=Require(Call(Native,"newFunctionWithName:",Text("main0")),"vertex entry main0");
            _fragmentFunction=Require(Call(_fragmentLibrary,"newFunctionWithName:",Text("main0")),"fragment entry main0");
        }
        catch{Dispose();throw;}
    }
    internal static string PrepareVertex(string source,string[] inputs)
    {
        for(int i=0;i<inputs.Length;i++)source=Regex.Replace(source,@"(?m)^(\s*)in\s+(\w+)\s+"+Regex.Escape(inputs[i])+@"\s*;","$1layout(location = "+i+") in $2 "+inputs[i]+";");
        return source;
    }
    private static nint CompileLibrary(MetalRenderBackend owner,string glsl,bool vertex,string[] inputs)
    {
        if(vertex)glsl=PrepareVertex(glsl,inputs);
        string msl=TranslateToMsl(glsl,vertex);nint error=0;
        nint lib=Call(owner.Device,"newLibraryWithSource:options:error:",Text(msl),0,(nint)(&error));
        if(lib==0)throw new InvalidOperationException("Metal MSL compilation failed: "+Error(error));return lib;
    }
    internal static string TranslateToMsl(string glsl,bool vertex)=>MetalSourceCompiler.Translate(glsl,vertex);
    internal nint Pipeline(VertexLayout layout,BlendState blend,nint colorFormat,nint depthFormat)
    {
        EnsureAlive();string key=$"{layout.Stride}:{blend}:{colorFormat}:{depthFormat}:"+string.Join(";",layout.Attributes.ToArray());
        if(_pipelines.TryGetValue(key,out nint p))return p;
        nint d=New("MTLRenderPipelineDescriptor"),v=New("MTLVertexDescriptor");
        try
        {
            Call(d,"setVertexFunction:",_vertexFunction);Call(d,"setFragmentFunction:",_fragmentFunction);
            foreach(var a in layout.Attributes.Span)
            {
                nint item=Call(Call(v,"attributes"),"objectAtIndexedSubscript:",a.Location);
                Call(item,"setFormat:",AttributeFormat(a));Call(item,"setOffset:",a.ByteOffset);Call(item,"setBufferIndex:",30);
            }
            nint binding=Call(Call(v,"layouts"),"objectAtIndexedSubscript:",30);
            Call(binding,"setStride:",layout.Stride);Call(binding,"setStepFunction:",1);Call(d,"setVertexDescriptor:",v);
            nint c=Call(Call(d,"colorAttachments"),"objectAtIndexedSubscript:",0);
            Call(c,"setPixelFormat:",colorFormat);Call(c,"setBlendingEnabled:",blend.Enabled?1:0);
            foreach(string channel in new[]{"RGB","Alpha"})
            {
                Call(c,"setSource"+channel+"BlendFactor:",Factor(blend.Source));Call(c,"setDestination"+channel+"BlendFactor:",Factor(blend.Destination));
                Call(c,"set"+(channel=="RGB"?"Rgb":"Alpha")+"BlendOperation:",(int)blend.Equation-1);
            }
            Call(d,"setDepthAttachmentPixelFormat:",depthFormat);if(depthFormat==260)Call(d,"setStencilAttachmentPixelFormat:",depthFormat);
            nint error=0;p=Call(Owner.Device,"newRenderPipelineStateWithDescriptor:error:",d,(nint)(&error));
            if(p==0)throw new InvalidOperationException("Metal pipeline creation failed: "+Error(error));_pipelines.Add(key,p);return p;
        }
        finally{Release(d);Release(v);}
    }
    private static nint AttributeFormat(VertexAttribute a)
    {
        if(a.ComponentCount<1||a.ComponentCount>4)throw new ArgumentException("Metal attribute component count must be 1..4.");
        if(a.Type==VertexAttributeType.Float)return 27+a.ComponentCount;
        if(a.ComponentCount==1)return a.Normalized?47:45;
        return(a.Normalized?7:1)+a.ComponentCount-2;
    }
    private static nint Factor(BlendFactor f)=>f switch
    {
        BlendFactor.Zero=>0,BlendFactor.One=>1,BlendFactor.SourceColor=>2,BlendFactor.InverseSourceColor=>3,BlendFactor.SourceAlpha=>4,BlendFactor.InverseSourceAlpha=>5,
        BlendFactor.DestinationColor=>6,BlendFactor.InverseDestinationColor=>7,BlendFactor.DestinationAlpha=>8,BlendFactor.InverseDestinationAlpha=>9,BlendFactor.SourceAlphaSaturate=>10,
        _=>throw new ArgumentOutOfRangeException(nameof(f))
    };
    public int GetUniformLocation(string name)=>-1;
    public void Use()=>EnsureAlive();
    public void SetUniform(int location,float value)=>Legacy(location);
    public void SetUniform(int location,float x,float y)=>Legacy(location);
    public void SetUniform(int location,float x,float y,float z)=>Legacy(location);
    public void SetUniform(int location,float x,float y,float z,float w)=>Legacy(location);
    public void SetUniform(int location,ReadOnlySpan<float> matrix4)=>Legacy(location);
    public void SetUniform(int location,int value)=>Legacy(location);
    private void Legacy(int location){EnsureAlive();if(location>=0)throw new ArgumentException("Metal uniforms use SetUniformBlock.");}
    public override void Dispose()
    {
        if(Native==0)return;foreach(nint p in _pipelines.Values)Release(p);_pipelines.Clear();Release(_vertexFunction);Release(_fragmentFunction);Release(_fragmentLibrary);base.Dispose();
    }
}
