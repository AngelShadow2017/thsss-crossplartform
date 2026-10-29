using System;
using SlimDX;
using SlimDX.Direct3D9;
using V4 = System.Numerics.Vector4;

// Camera math parity for the stage camera, checked entirely on the CPU through the same
// SlimDX Matrix API the game calls. This pins conventions (left-handed, D3D depth range)
// but proves nothing about what the GPU does with these matrices - the NDC expectations
// are values recorded from the original D3D9 render, not rasteriser output.
static void Near(float actual, float expected, string name)
{
    // 0.002 is loose enough for the float chain through view*projection yet tight enough
    // to catch a wrong sign or a transposed row.
    if (MathF.Abs(actual - expected) > 0.002f) throw new Exception($"{name}: {actual} != {expected}");
}
// Stage camera positions taken from the original game's scene setup. Keeping the real
// numbers is what makes the NDC expectations below meaningful rather than self-referential.
var eye = new Vector3(0, 2200, 130);
var target = new Vector3(0, 2400, 50);
var view = Matrix.LookAtLH(eye, target, Vector3.UnitY);
var projection = Matrix.PerspectiveFovLH(MathF.PI / 2, 1, 1, 10000);
V4 Project(Vector3 p) => WorldViewProjection.Project(Matrix.Identity.ToSystemMatrix(), view.ToSystemMatrix(), projection.ToSystemMatrix(), new V4(p.X,p.Y,p.Z,1));
var centre = Project(target);
// The look target must land exactly at NDC centre. Any handedness or LookAt convention
// error moves it off-centre immediately.
Near(centre.X / centre.W, 0, "target x");
Near(centre.Y / centre.W, 0, "target y");
// Positive clip W is the left-handed requirement; an RH or transposed matrix flips it and
// every visible vertex would be culled.
if (centre.W <= 0) throw new Exception("LH target behind camera");
var origin = Vector3.TransformCoordinate(eye, view);
// The view transform maps the eye to the origin: no residual translation on any axis.
Near(origin.X,0,"camera origin x"); Near(origin.Y,0,"camera origin y"); Near(origin.Z,0,"camera origin z");
var clip = Project(new Vector3(-500,6000,0));
// Arbitrary-looking constants are recorded from the original D3D9 render of this terrain
// vertex. They exist to catch a sign or transposition regression, not to be recomputed
// by hand - and a non-zero result also proves the term is not projected to the centre.
Near(clip.X / clip.W, 0.1398015f, "stage terrain x");
Near(clip.Y / clip.W, 0.360867f, "stage terrain y");
var lh = Matrix.PerspectiveFovLH(MathF.PI/2,1,1,100);
var near = Vector3.TransformCoordinate(new Vector3(0,0,1),lh);
var far = Vector3.TransformCoordinate(new Vector3(0,0,100),lh);
// D3D9 depth convention: near plane maps to z=0, far plane to z=1 (GL would be -1..+1).
// The port keeps a left-handed projection, so these are the values that must be preserved.
Near(near.Z,0,"LH near depth"); Near(far.Z,1,"LH far depth");
var rh = Matrix.LookAtRH(eye,target,Vector3.UnitY);
var rhTarget = Vector3.TransformCoordinate(target,rh);
// RH is only used as a contrast case: the same target sits behind the camera (negative view
// Z). Mixing LH and RH is the classic porting bug, so both are asserted side by side.
if (rhTarget.Z >= 0) throw new Exception("RH target should have negative view depth");
Near(rhTarget.X,0,"RH target x"); Near(rhTarget.Y,0,"RH target y");
Console.WriteLine("PASS: stage camera target/origin, nonzero terrain NDC, positive clip W, LH near/far and RH handedness");
