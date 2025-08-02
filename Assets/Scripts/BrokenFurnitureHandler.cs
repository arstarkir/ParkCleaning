using Unity.Netcode;
using System.Collections.Generic;
using UnityEngine;

public class BrokenFurnitureHandler : NetworkBehaviour
{
    public List<GameObject> brokenObj = new List<GameObject>();
}
