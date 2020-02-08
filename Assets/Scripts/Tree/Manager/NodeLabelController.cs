using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NodeLabelController : MonoBehaviour
{
   public Text rankLabelText;
   public Text nameLabelText;

   public void UpdateLabelVisuals(string nodeName, string nodeRank)
   {
      rankLabelText.text = nodeRank;
      nameLabelText.text = nodeName;
   }
}
