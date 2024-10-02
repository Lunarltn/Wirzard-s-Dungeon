using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteraction
{
    public void Interect();
    public string GetInterectionName();
    public string GetInterectionMessage();
    public void SetPosition(Vector3 position);
}
