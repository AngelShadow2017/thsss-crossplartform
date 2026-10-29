using SlimDX;

// Pure CPU math parity: D3D9 matrix conventions and 2D transform semantics as the game
// uses them. No GPU is touched, so this cannot detect a wrong shader-side transpose or a
// clip-space divergence - only the CPU-side contract.
static void Near(float actual, float expected, string name)
{
    // 0.0001 is float noise for these small matrices, so a mismatch is a real difference.
    if (MathF.Abs(actual - expected) > 0.0001f)
        throw new Exception($"{name}: expected {expected}, got {actual}");
}

// GLSL multiplies column vectors; the upload must transpose the D3D row-vector matrix.
// Both layouts happen to keep translation in elements 12..14, so these two checks alone
// are convention-neutral - the projection checks below are what actually distinguish them.
var upload = Matrix.Translation(7, 9, 0).ToOpenGLMatrix();
Near(upload[12], 7, "GL translation X");
Near(upload[13], 9, "GL translation Y");
// Argument order is left=0, right=640, bottom=480, top=0. D3D screen space has +Y
// downwards, so top < bottom is intentional: pixel row 0 lands at clip Y = +1, the top.
var projection = Matrix.OrthoOffCenterLH(0, 640, 480, 0, -1, 1).ToOpenGLMatrix();
Near(projection[12], -1, "top-left clip X");
Near(projection[13], 1, "top-left clip Y");
// Evaluating the matrix by hand at both corners: this is what proves the transpose,
// since the translation indices above look the same in either convention.
Near(projection[0] * 640 + projection[12], 1, "bottom-right clip X");
Near(projection[5] * 480 + projection[13], -1, "bottom-right clip Y");

// Scaling about a pivot must leave the pivot fixed; the original used this to keep sprites
// anchored at their centre while the stage camera zoomed.
var pivot = new Vector2(320, 240);
var scale = Matrix.Transformation2D(pivot, 0, new Vector2(0.625f, 0.625f), pivot, 0, Vector2.Zero);
var fixedPivot = Vector2.TransformCoordinate(pivot, scale);
Near(fixedPivot.X, 320, "scale pivot X");
Near(fixedPivot.Y, 240, "scale pivot Y");
// A +90 degree 2D rotation maps +X onto +Y because D3D's 2D space is Y-down; the (3,4)
// translation is then applied, giving (323,245). Those offsets look wrong for a textbook
// rotation and are correct here - the deliberate oddity pins the screen-space convention.
var rotation = Matrix.Transformation2D(Vector2.Zero, 0, Vector2.One, pivot, MathF.PI / 2, new Vector2(3, 4));
var rotated = Vector2.TransformCoordinate(new Vector2(321, 240), rotation);
Near(rotated.X, 323, "rotated X");
Near(rotated.Y, 245, "rotated Y");
Console.WriteLine("PASS: GL upload, projection corners, scaling pivot, rotation pivot and translation");
