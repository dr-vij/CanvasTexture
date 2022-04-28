using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


public class CustomVE : VisualElement
{
    public new class UxmlFactory : UxmlFactory<CustomVE, UxmlTraits> { }

    [SerializeField] private VisualTreeAsset m_VisualTeeAsset = default;

    public CustomVE()
    {
        var tree = m_VisualTeeAsset.Instantiate();
    }
}