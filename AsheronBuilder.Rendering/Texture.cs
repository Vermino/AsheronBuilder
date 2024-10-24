// using System.Drawing;
// using System.Drawing.Imaging;
// using OpenTK.Graphics.OpenGL4;
// using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;
//
// namespace AsheronBuilder.Rendering
// {
//     public class Texture
//     {
//         public int Handle { get; private set; }
//
//         public Texture(string path)
//         {
//             Handle = GL.GenTexture();
//             Use();
//
//             using (var image = new Bitmap(path))
//             {
//                 var data = image.LockBits(
//                     new Rectangle(0, 0, image.Width, image.Height),
//                     ImageLockMode.ReadOnly,
//                     System.Drawing.Imaging.PixelFormat.Format32bppArgb);
//
//                 GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
//             }
//
//             GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
//             GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
//
//             GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
//             GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
//
//             GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
//         }
//
//         public void Use(TextureUnit unit = TextureUnit.Texture0)
//         {
//             GL.ActiveTexture(unit);
//             GL.BindTexture(TextureTarget.Texture2D, Handle);
//         }
//
//         public void Dispose()
//         {
//             GL.DeleteTexture(Handle);
//         }
//     }
// }