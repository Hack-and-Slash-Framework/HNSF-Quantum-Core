using System;
using UnityEditor;
using Photon.Deterministic;
using Quantum.Editor;

namespace HnSF
{
    public class FPConverter : EditorWindow
    {
        private float _f;
        private FP _fp;

        [MenuItem("Tools/HnSF/FP Converter")]
        public static void ShowWindow()
        {
            GetWindow(typeof(FPConverter), false, "FP Converter");
        }

        public virtual void OnGUI()
        {
            _f = EditorGUILayout.FloatField("Input", _f);
            try
            {
                _fp = FP.FromFloat_UNSAFE(_f);
                var f = _fp.AsFloat; //FPPropertyDrawer.GetRawAsFloat(_fp.RawValue);
                var rect = EditorGUILayout.GetControlRect(true);
                EditorGUI.FloatField(rect, "Output FP", f);
                QuantumEditorGUI.Overlay(rect, "(FP)");
                EditorGUILayout.LongField("Output Raw", _fp.RawValue);
            }
            catch (OverflowException)
            {
                EditorGUILayout.LabelField("Out of range");
            }
        }
    }
}