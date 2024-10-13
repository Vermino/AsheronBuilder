using OpenTK.Mathematics;

namespace ACDungeonBuilder.Rendering
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
    }
}