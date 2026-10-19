using SlimDX;

static void Near(float actual, float expected, string name)
{
    if (MathF.Abs(actual - expected) > 0.0001f)
        throw new Exception($"{name}: expected {expected}, got {actual}");
}

// GLSL multiplies column vectors; the upload must transpose the D3D row-vector matrix.
var upload = Matrix.Translation(7, 9, 0).ToOpenGLMatrix();
Near(upload[12], 7, "GL translation X");
Near(upload[13], 9, "GL translation Y");
var projection = Matrix.OrthoOffCenterLH(0, 640, 480, 0, -1, 1).ToOpenGLMatrix();
Near(projection[12], -1, "top-left clip X");
Near(projection[13], 1, "top-left clip Y");
Near(projection[0] * 640 + projection[12], 1, "bottom-right clip X");
Near(projection[5] * 480 + projection[13], -1, "bottom-right clip Y");

var pivot = new Vector2(320, 240);
var scale = Matrix.Transformation2D(pivot, 0, new Vector2(0.625f, 0.625f), pivot, 0, Vector2.Zero);
var fixedPivot = Vector2.TransformCoordinate(pivot, scale);
Near(fixedPivot.X, 320, "scale pivot X");
Near(fixedPivot.Y, 240, "scale pivot Y");
var rotation = Matrix.Transformation2D(Vector2.Zero, 0, Vector2.One, pivot, MathF.PI / 2, new Vector2(3, 4));
var rotated = Vector2.TransformCoordinate(new Vector2(321, 240), rotation);
Near(rotated.X, 323, "rotated X");
Near(rotated.Y, 245, "rotated Y");
Console.WriteLine("PASS: GL upload, projection corners, scaling pivot, rotation pivot and translation");
