using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IUserInteractable
{
  void HandleInputOccur(RaycastHit hit);
  
  void HandleInputClickOccur();
  
  void HandleInputNotOccur();
}
