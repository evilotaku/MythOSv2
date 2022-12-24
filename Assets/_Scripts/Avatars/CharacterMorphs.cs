using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMorphs : MonoBehaviour
{
    [System.Serializable]
    public class Morph 
    {
        [HideInInspector]
        public string morphName;
        [Range(0,2)]
        public float value; 

        public Morph(string _name, float _value)
        {
            morphName = _name;
            value = _value;
        }
       
    }
    [NonReorderable]
    public Morph[] blendShapes;   

    static void ArrayEditor<T>(IEnumerable<T> items)
    {
        if(items == null) return;
        var name = typeof(T).GetField("morphName");
        foreach(var i in items)
            name.SetValue(i,i.ToString());
    }

    void OnValidate()
    {
        ArrayEditor(blendShapes);
        SetBlendShapes();
        GetBlendShapes();        
    }    
    void GetBlendShapes()
    {        
        var renderer = GetComponentInChildren<SkinnedMeshRenderer>();                 
        for( int i = 0; i < renderer.sharedMesh.blendShapeCount; i++)
        {
            blendShapes[i] = new Morph(renderer.sharedMesh.GetBlendShapeName(i),
                renderer.GetBlendShapeWeight(i));            
        }
    }

    [ContextMenu("Clear All Blendshapes")]
    void ClearBlendShapes()
    {
        blendShapes = new Morph[blendShapes.Length];
        foreach(var renderer in GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            for( int i = 0; i < renderer.sharedMesh.blendShapeCount; i++)
            {
                renderer.SetBlendShapeWeight(i, 0);
            }
        }

        GetBlendShapes();
    }

    void SetBlendShapes()
    {        
        foreach(var renderer in GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            for( int i = 0; i < blendShapes.Length; i++)
            {
                renderer.SetBlendShapeWeight(i, blendShapes[i].value);
            }
        }
    }
}
