using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NodeLabelController : MonoBehaviour
{
   public Text nameLabelText;

   public void UpdateLabelText(string nodeName)
   {
      nameLabelText.text = nodeName;
   }
}
