using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ApplyTextureNode : Unit
{
    protected ControlInput m_InputTrigger;
    protected ControlOutput m_OutputTrigger;

    protected ValueInput m_InputData;

    private RenderTextureData m_Data;

    protected override void Definition()
    {
        m_InputTrigger = ControlInput("Input",
           (flow) =>
           {
               m_Data = flow.GetValue<RenderTextureData>(m_InputData);
               m_Data.Flush();
               return m_OutputTrigger;
           });

        m_OutputTrigger = ControlOutput("Output");
        m_InputData = ValueInput<RenderTextureData>("DataInput");
    }
}
