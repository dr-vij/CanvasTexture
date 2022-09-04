using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public abstract class RenderTextureBaseNode : Unit
{
    protected ControlInput m_InputTrigger;
    protected ControlOutput m_OutputTrigger;

    protected ValueInput m_InputData;
    protected ValueOutput m_OutputData;

    private RenderTextureData m_Data;

    protected override void Definition()
    {
        m_InputTrigger = ControlInput("Input",
           (flow) =>
           {
               m_Data = flow.GetValue<RenderTextureData>(m_InputData);
               Paint(m_Data);
               return m_OutputTrigger;
           });

        m_OutputTrigger = ControlOutput("Output");

        m_InputData = ValueInput<RenderTextureData>("DataInput");
        m_OutputData = ValueOutput("DataOutput", (flow) => m_Data);

        Requirement(m_InputData, m_InputTrigger);
        Succession(m_InputTrigger, m_OutputTrigger);
    }

    protected virtual void Paint(RenderTextureData data)
    {
    }
}
