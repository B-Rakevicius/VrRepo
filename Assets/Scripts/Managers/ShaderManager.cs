using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShaderManager : MonoBehaviour
{

    public static ShaderManager instance;
    public Shader damageShader;
    public Shader normalShader;
    public Material whiteMountain, grayMountain, rockMountain, sandWater;
    public Mesh aoeMesh;
    
    private void Awake()
    {
        normalShader = Shader.Find("HDRP/Lit");
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }
}   