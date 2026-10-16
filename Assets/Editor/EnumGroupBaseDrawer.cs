//AI
#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Common.Editor
{
    [CustomPropertyDrawer(typeof(EnumGroupBase), true)]
    public class EnumGroupBaseDrawer : PropertyDrawer
    {
        private const float VerticalSpacing = 2f;
        private const float SelectorHeight = 18f;

        public override void OnGUI(
            Rect position,
            SerializedProperty property,
            GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            float lineHeight = EditorGUIUtility.singleLineHeight;

            Rect foldoutRect = new(
                position.x,
                position.y,
                position.width,
                lineHeight);

            property.isExpanded = EditorGUI.Foldout(
                foldoutRect,
                property.isExpanded,
                GetHeaderLabel(property, label),
                true);

            if (property.isExpanded)
            {
                float y = position.y + lineHeight + VerticalSpacing;

                Rect selectorRect = new(
                    position.x + EditorGUIUtility.singleLineHeight,
                    y,
                    position.width - EditorGUIUtility.singleLineHeight,
                    SelectorHeight);

                DrawTypeSelector(selectorRect, property);

                y += SelectorHeight + VerticalSpacing;

                DrawChildren(
                    new Rect(
                        position.x + EditorGUIUtility.singleLineHeight,
                        y,
                        position.width - EditorGUIUtility.singleLineHeight,
                        position.height - (y - position.y)),
                    property);
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(
            SerializedProperty property,
            GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight;

            if (!property.isExpanded)
            {
                return height;
            }

            height += VerticalSpacing;
            height += SelectorHeight;
            height += VerticalSpacing;
            height += GetChildrenHeight(property);

            return height;
        }

        private static GUIContent GetHeaderLabel(
            SerializedProperty property,
            GUIContent label)
        {
            if (property.managedReferenceValue == null)
            {
                return label;
            }

            string groupName = property.managedReferenceValue
                .GetType()
                .Name;

            SerializedProperty typeProperty =
                property.FindPropertyRelative("_type");

            if (typeProperty == null)
            {
                return new GUIContent(
                    $"{label.text}    ({groupName})");
            }

            string typeName = typeProperty.propertyType == SerializedPropertyType.Enum
                ? typeProperty.enumDisplayNames[typeProperty.enumValueIndex]
                : typeProperty.displayName;

            return new GUIContent(
                $"{label.text}    {groupName} / {typeName}");
        }

        private static void DrawTypeSelector(
            Rect position,
            SerializedProperty property)
        {
            string currentName = GetCurrentTypeName(property);

            if (EditorGUI.DropdownButton(
                    position,
                    new GUIContent(currentName),
                    FocusType.Keyboard))
            {
                ShowTypeMenu(property);
            }
        }

        private static string GetCurrentTypeName(
            SerializedProperty property)
        {
            if (property.managedReferenceValue == null)
            {
                return "Select Group";
            }

            return property.managedReferenceValue
                .GetType()
                .Name;
        }

        private static void ShowTypeMenu(
            SerializedProperty property)
        {
            GenericMenu menu = new();

            List<Type> types = GetDerivedTypes();

            foreach (Type type in types)
            {
                string name = type.Name;

                bool isCurrent =
                    property.managedReferenceValue != null &&
                    property.managedReferenceValue.GetType() == type;

                menu.AddItem(
                    new GUIContent(name),
                    isCurrent,
                    () => SetManagedReference(property, type));
            }

            if (types.Count == 0)
            {
                menu.AddDisabledItem(
                    new GUIContent("No derived types found"));
            }

            menu.ShowAsContext();
        }

        private static void SetManagedReference(
            SerializedProperty property,
            Type type)
        {
            property.serializedObject.Update();

            property.managedReferenceValue =
                Activator.CreateInstance(type);

            property.serializedObject.ApplyModifiedProperties();

            property.isExpanded = true;

            GUI.changed = true;
        }

        private static List<Type> GetDerivedTypes()
        {
            return TypeCache
                .GetTypesDerivedFrom<EnumGroupBase>()
                .Where(type =>
                    !type.IsAbstract &&
                    !type.IsGenericType &&
                    type.GetConstructor(Type.EmptyTypes) != null)
                .OrderBy(type => type.Name)
                .ToList();
        }

        private static void DrawChildren(
            Rect position,
            SerializedProperty property)
        {
            if (property.managedReferenceValue == null)
            {
                return;
            }

            SerializedProperty iterator = property.Copy();
            SerializedProperty endProperty = iterator.GetEndProperty();

            bool enterChildren = true;
            float y = position.y;

            while (iterator.NextVisible(enterChildren) &&
                   !SerializedProperty.EqualContents(iterator, endProperty))
            {
                float height = EditorGUI.GetPropertyHeight(
                    iterator,
                    true);

                Rect childRect = new(
                    position.x,
                    y,
                    position.width,
                    height);

                EditorGUI.PropertyField(
                    childRect,
                    iterator,
                    true);

                y += height + VerticalSpacing;

                enterChildren = false;
            }
        }

        private static float GetChildrenHeight(
            SerializedProperty property)
        {
            if (property.managedReferenceValue == null)
            {
                return 0f;
            }

            SerializedProperty iterator = property.Copy();
            SerializedProperty endProperty = iterator.GetEndProperty();

            bool enterChildren = true;
            float height = 0f;

            while (iterator.NextVisible(enterChildren) &&
                   !SerializedProperty.EqualContents(iterator, endProperty))
            {
                height += EditorGUI.GetPropertyHeight(
                    iterator,
                    true);

                height += VerticalSpacing;

                enterChildren = false;
            }

            return height;
        }
    }
}

#endif