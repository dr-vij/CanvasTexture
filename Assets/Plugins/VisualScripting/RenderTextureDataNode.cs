using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class RenderTextureDataNode : Unit
{
    protected ValueOutput m_OutputData;
    private RenderTextureData m_Data;

    protected override void Definition()
    {
        m_OutputData = ValueOutput("DataOutput", (flow) =>
        {
            if (m_Data == null)
            {
                m_Data = new RenderTextureData();
            }

            m_Data.Init(new RenderTextureDescriptor(128, 128));

            return m_Data;
        });
    }
}
