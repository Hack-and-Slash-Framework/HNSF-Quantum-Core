using UnityEditor;

// Source: https://github.com/aarthificial-unity/typewriter
namespace HnSF
{
    public static class SerializedPropertyExtensions
    {
        internal static bool Update(
            this SerializedProperty property,
            ref SerializedProperty reference
        )
        {
            var same = reference == property;
            reference = property;
            return !same;
        }

        internal static SerializedProperty FirstString(
            this SerializedProperty property
        )
        {
            if (property.propertyType == SerializedPropertyType.String)
            {
                return property;
            }

            var child = property.Copy();
            if (child.Next(true))
            {
                do
                {
                    if (child.propertyType == SerializedPropertyType.String)
                    {
                        return child;
                    }
                } while (child.Next(false));
            }

            return null;
        }
    }
}