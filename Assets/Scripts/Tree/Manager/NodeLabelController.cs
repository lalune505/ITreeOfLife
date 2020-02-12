using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NodeLabelController : MonoBehaviour
{
   public Text idLabelText;
   public Text nameLabelText;

   public void UpdateLabelVisuals(string nodeId, string nodeName)
   {
      idLabelText.text = nodeId;
      nameLabelText.text = nodeName;
   }
}
