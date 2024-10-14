using OpenTK.Mathematics;

namespace AsheronBuilder.Rendering
{
    public class LevelOfDetail
    {
        public static int GetLODLevel(Vector3 objectPosition, Vector3 cameraPosition, float[] lodDistances)
        {
            float distance = Vector3.Distance(objectPosition, cameraPosition);
            
            for (int i = 0; i < lodDistances.Length; i++)
            {
                if (distance <= lodDistances[i])
                {
                    return i;
                }
            }

            return lodDistances.Length;
        }

        public static float GetLODBlendFactor(Vector3 objectPosition, Vector3 cameraPosition, float[] lodDistances)
        {
            float distance = Vector3.Distance(objectPosition, cameraPosition);
            
            for (int i = 0; i < lodDistances.Length - 1; i++)
            {
                if (distance <= lodDistances[i + 1])
                {
                    float t = (distance - lodDistances[i]) / (lodDistances[i + 1] - lodDistances[i]);
                    return MathHelper.Clamp(t, 0, 1);
                }
            }

            return 1;
        }
    }
}