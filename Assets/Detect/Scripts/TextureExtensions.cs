using UnityEngine;

public static class TextureExtensions
{
    public static Texture2D Resized(this Texture2D source, int width, int height)
    {
        // 1. Create a temporary RenderTexture with the target dimensions
        RenderTexture rt = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
        // rt.useMipMap = false;
        // 2. Set the filtering mode for scaling quality (Bilinear or Point for pixel art)
        source.filterMode = FilterMode.Bilinear;
        rt.filterMode = FilterMode.Bilinear;

        // 3. Blit (copy/scale) the source texture into the RenderTexture
        Graphics.Blit(source, rt);

        // 4. Save the current active RenderTexture so we can restore it later
        RenderTexture previousActive = RenderTexture.active;
        RenderTexture.active = rt;

        // 5. Read the pixels from the active RenderTexture into a new Texture2D
        Texture2D result = new Texture2D(width, height, TextureFormat.ARGB32, false);
        result.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        result.Apply();

        // 6. Clean up and restore states to prevent memory leaks
        RenderTexture.active = previousActive;
        RenderTexture.ReleaseTemporary(rt);

        return result;
    }
}
