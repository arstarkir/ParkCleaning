using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RelicRegister", menuName = "SO/RelicRegister")]
public class RelicRegister : ScriptableObject
{
    public List<RelicData> relicDatas = new List<RelicData>();
}
