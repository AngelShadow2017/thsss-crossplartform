using System;
using SlimDX;
using SlimDX.Direct3D9;
using V4 = System.Numerics.Vector4;

static void Near(float actual, float expected, string name)
{
    if (MathF.Abs(actual - expected) > 0.002f) throw new Exception($"{name}: {actual} != {expected}");
}
var eye = new Vector3(0, 2200, 130);
var target = new Vector3(0, 2400, 50);
var view = Matrix.LookAtLH(eye, target, Vector3.UnitY);
var projection = Matrix.PerspectiveFovLH(MathF.PI / 2, 1, 1, 10000);
V4 Project(Vector3 p) => WorldViewProjection.Project(Matrix.Identity.ToSystemMatrix(), view.ToSystemMatrix(), projection.ToSystemMatrix(), new V4(p.X,p.Y,p.Z,1));
var centre = Project(target);
Near(centre.X / centre.W, 0, "target x");
Near(centre.Y / centre.W, 0, "target y");
if (centre.W <= 0) throw new Exception("LH target behind camera");
var origin = Vector3.TransformCoordinate(eye, view);
Near(origin.X,0,"camera origin x"); Near(origin.Y,0,"camera origin y"); Near(origin.Z,0,"camera origin z");
var clip = Project(new Vector3(-500,6000,0));
Near(clip.X / clip.W, 0.1398015f, "stage terrain x");
Near(clip.Y / clip.W, 0.360867f, "stage terrain y");
var lh = Matrix.PerspectiveFovLH(MathF.PI/2,1,1,100);
var near = Vector3.TransformCoordinate(new Vector3(0,0,1),lh);
var far = Vector3.TransformCoordinate(new Vector3(0,0,100),lh);
Near(near.Z,0,"LH near depth"); Near(far.Z,1,"LH far depth");
var rh = Matrix.LookAtRH(eye,target,Vector3.UnitY);
var rhTarget = Vector3.TransformCoordinate(target,rh);
if (rhTarget.Z >= 0) throw new Exception("RH target should have negative view depth");
Near(rhTarget.X,0,"RH target x"); Near(rhTarget.Y,0,"RH target y");
Console.WriteLine("PASS: stage camera target/origin, nonzero terrain NDC, positive clip W, LH near/far and RH handedness");
