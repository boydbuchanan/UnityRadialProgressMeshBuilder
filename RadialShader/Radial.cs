using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshRenderer))]
public class Radial : MonoBehaviour
{
    [SerializeField]
    [RangeAttribute(0, 1)]
    public float Percent;

    private float _growPercent;

    [Header("Radial Properties")]
    public Material mat;
    private MeshRenderer MeshRenderer => GetComponent<MeshRenderer>();

    // Start is called before the first frame update
    void Awake()
    {
        if(mat == null || !mat.HasProperty("_Grow")){
            Debug.LogError("Invalid Radial Material");
        }
        MeshRenderer.material = mat;
    }

    // Update is called once per frame
    void Update()
    {
        if(_growPercent != Percent){
            _growPercent = Percent;
            UpdateGrow(_growPercent);
        }
    }

    public void SetPercent(float percent)
    {
        Percent = percent;
    }

    private void UpdateGrow(float percent)
    {
        mat.SetFloat("_Grow", percent);
    }

    [ContextMenu("Update Percent")]
    protected void GenerateMeshMenu()
    {
        SetPercent(Percent);
    }

    private float rotation = 0;

    [ContextMenu("Invert")]
    protected void Invert()
    {
        rotation = rotation == 0 ? 180 : 0;
        this.transform.Rotate(0, 0, rotation);
    }
}
