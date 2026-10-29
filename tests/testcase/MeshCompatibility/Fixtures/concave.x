xof 0303txt 0032
Material Red { 1;0;0;1;; 8; 0.2;0.3;0.4;; 0;0;0;; TextureFilename { "red.bmp"; } }
Material Green { 0;1;0;1;; 4; 0;0;0;; 0;0;0;; }
Material Unused { 0;0;1;1;; 0; 0;0;0;; 0;0;0;; }
Frame Parent {
 Frame Child {
  Mesh Concave {
   6;
   -0.8;-0.8;0.5;, 0.8;-0.8;0.5;, 0.8;0;0.5;, 0;0;0.5;, 0;0.8;0.5;, -0.8;0.8;0.5;;
   2;
   6;0,1,2,3,4,5;,
   3;0,2,3;;
   MeshTextureCoords { 6; 0;1;,1;1;,1;0.5;,0.5;0.5;,0.5;0;,0;0;; }
   MeshMaterialList { 3; 2; 0,1;; { Red } { Green } { Unused } }
  }
 }
}
