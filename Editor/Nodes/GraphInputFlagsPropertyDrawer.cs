#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;

namespace Quantum
{
    [CustomPropertyDrawer(typeof(ActorInputButtonType))]
    public class TransitionLogicFlagsDrawer : EnumFlagsDrawer<ActorInputButtonType> { }


    public abstract class EnumFlagsDrawer<T> : PropertyDrawer where T : Enum
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //base.OnGUI(position, property, label);
            EditorGUI.PropertyField(position, property, new GUIContent("Input"));
        }

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var container = new VisualElement();
            var currentEnum = (T)Enum.ToObject(typeof(T), property.enumValueFlag);
            var enumField   = new EnumFlagsField("", currentEnum); //property.displayName
            enumField.RegisterValueChangedCallback(evt =>
            {
                property.enumValueFlag = Convert.ToInt32(evt.newValue);
                property.serializedObject.ApplyModifiedProperties();
            });
        
            container.Add(enumField);
            return container;
        }
    }
}
#endif