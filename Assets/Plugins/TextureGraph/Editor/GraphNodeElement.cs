using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.IO;

namespace ViJ.GraphEditor
{
    public class GraphNodeElement : VisualElement
    {
        private const string UXML = nameof(GraphNodeElement) + ".uxml";
        private const string SELECTED_NODE_CLASS = "nodeSelected";
        private const string PRESELECTED_NODE_CLASS = "nodePreselected";
        private const string UNSELECTED_NODE_CLASS = "nodeUnselected";

        public new class UxmlFactory : UxmlFactory<GraphNodeElement, UxmlTraits> { }
        public new class UxmlTraits : VisualElement.UxmlTraits { }

        private int m_ID = -1;
        private bool mIsSelected = false;
        private bool mIsPreselected = false;
        private VisualElement m_Node;

        public int ID
        {
            get => m_ID;
            set
            {
                if (m_ID == -1)
                {
                    if (value >= 0)
                        m_ID = value;
                    else
                        throw new System.Exception("Incorrect ID");
                }
                else
                {
                    throw new System.Exception("ID must not be changed");
                }
            }
        }

        public bool HasId => m_ID != -1;

        public Rect NodeWorldBounds => m_Node.worldBound;

        public bool IsSelected
        {
            get => mIsSelected;
            set
            {
                if (mIsSelected != value)
                {
                    mIsSelected = value;
                    UpdateView();
                }
            }
        }

        public bool IsPreSelected
        {
            get => mIsPreselected;
            set
            {
                if (mIsPreselected != value)
                {
                    mIsPreselected = value;
                    UpdateView();
                }
            }
        }

        public GraphNodeElement()
        {
            var asset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Path.Combine(TextureGraphSettings.PLUGIN_PATH, "Editor", UXML));
            Add(asset.Instantiate());
            m_Node = this.Q<VisualElement>("Node");

            UpdateView();
        }

        private void UpdateView()
        {
            //node has unselected style if it is not selected or preselected
            if (!mIsSelected && !mIsPreselected)
                m_Node.AddToClassList(UNSELECTED_NODE_CLASS);
            else if (mIsSelected || mIsPreselected)
                m_Node.RemoveFromClassList(UNSELECTED_NODE_CLASS);

            //preselected has higher priority
            if (mIsPreselected)
                m_Node.AddToClassList(PRESELECTED_NODE_CLASS);
            else
                m_Node.RemoveFromClassList(PRESELECTED_NODE_CLASS);

            //selected has lowest priority
            if (mIsSelected && !mIsPreselected)
                m_Node.AddToClassList(SELECTED_NODE_CLASS);
            else
                m_Node.RemoveFromClassList(SELECTED_NODE_CLASS);
        }
    }
}