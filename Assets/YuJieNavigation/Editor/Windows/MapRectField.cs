using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace YuJie.Navigation.Editors
{
    public class MapRectField : VisualElement
    {
        private IntegerField m_leftField;
        private IntegerField m_rightField;
        private IntegerField m_topField;
        private IntegerField m_bottomField;

        private RectInt m_Value;
        public RectInt value
        {
            get => m_Value;
            set
            {
                m_Value = value;
                UpdateFieldValues();
                OnValueChanged?.Invoke(m_Value);
            }
        }

        public event System.Action<RectInt> OnValueChanged;

        public MapRectField()
        {
            // 创建样式类
            AddToClassList("rect-int-field");

            // 创建字段容器
            var fieldsContainer = new VisualElement { name = "rect-int-fields" };
            fieldsContainer.AddToClassList("fields-container");

            // 创建字段
            m_leftField = CreateIntegerField("左边界:", 0, "x-field");
            m_rightField = CreateIntegerField("右边界:", 0, "y-field");
            m_topField = CreateIntegerField("上边界:", 0, "width-field");
            m_bottomField = CreateIntegerField("下边界:", 0, "height-field");

            // 添加到容器
            fieldsContainer.Add(m_leftField);
            fieldsContainer.Add(m_rightField);
            fieldsContainer.Add(m_topField);
            fieldsContainer.Add(m_bottomField);

            Add(fieldsContainer);

            // 注册回调
            RegisterFieldCallbacks();

            // 初始化值
            value = new UnityEngine.RectInt(0, 0, 100, 100);
        }

        public new class UxmlFactory : UxmlFactory<MapRectField, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private readonly UxmlIntAttributeDescription m_leftAttr = new UxmlIntAttributeDescription { name = "左边界", defaultValue = 0 };
            private readonly UxmlIntAttributeDescription m_rightAttr = new UxmlIntAttributeDescription { name = "右边界", defaultValue = 0 };
            private readonly UxmlIntAttributeDescription m_topAttr = new UxmlIntAttributeDescription { name = "上边界", defaultValue = 0 };
            private readonly UxmlIntAttributeDescription m_bottomAttr = new UxmlIntAttributeDescription { name = "下边界", defaultValue = 0 };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var field = ve as MapRectField;
                if (field == null)
                    return;

                field.value = new RectInt(
                    m_leftAttr.GetValueFromBag(bag, cc),
                    m_rightAttr.GetValueFromBag(bag, cc),
                    m_topAttr.GetValueFromBag(bag, cc),
                    m_bottomAttr.GetValueFromBag(bag, cc)
                );

            }
        }

        // 创建整数输入字段
        private IntegerField CreateIntegerField(string label, int defaultValue, string className)
        {
            var field = new IntegerField(label);
            field.AddToClassList(className);
            field.AddToClassList("rect-int-value");
            field.value = defaultValue;
            return field;
        }

        // 注册字段变更回调
        private void RegisterFieldCallbacks()
        {
            m_leftField.RegisterValueChangedCallback(e => UpdateRectFromFields());
            m_rightField.RegisterValueChangedCallback(e => UpdateRectFromFields());
            m_topField.RegisterValueChangedCallback(e => UpdateRectFromFields());
            m_bottomField.RegisterValueChangedCallback(e => UpdateRectFromFields());
        }

        // 更新字段值显示
        private void UpdateFieldValues()
        {
            m_leftField.SetValueWithoutNotify(value.x);
            m_rightField.SetValueWithoutNotify(value.y);
            m_topField.SetValueWithoutNotify(value.width);
            m_bottomField.SetValueWithoutNotify(value.height);
        }

        // 根据字段更新Rect值
        private void UpdateRectFromFields()
        {
            value = new RectInt(
                m_leftField.value,
                m_rightField.value,
                m_topField.value,
                m_bottomField.value
            );
        }
    }
}
