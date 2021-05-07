using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CBGames
{
    public static class Extensions
    {
        public static Vector3 Round(this Vector3 vector3, int decimalPlaces = 5)
        {
            float multiplier = 1;
            for (int i = 0; i < decimalPlaces; i++)
            {
                multiplier *= 10f;
            }
            return new Vector3(
                Mathf.Round(vector3.x * multiplier) / multiplier,
                Mathf.Round(vector3.y * multiplier) / multiplier,
                Mathf.Round(vector3.z * multiplier) / multiplier);
        }

        public static bool ContainsNull(this List<Component> input)
        {
            if (input == null)
            {
                return true;
            }
            else
            {
                return input.Any(i => i == null);
            }
        }

        public static bool Contains(this LayerMask layerMask, string layerName)
        {
            int inputLayer = LayerMask.NameToLayer(layerName);
            return layerMask == (layerMask | (1 << inputLayer));
        }
    }
}