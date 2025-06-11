
using UnityEngine;
using UnityEngine.UI;

public class WipeConroller : MonoBehaviour
{

    private Animator _animator;
    private Image _image;
    private readonly int _circleSizeId = Shader.PropertyToID("_Circle_Size");

    public float circleSize = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _animator = GetComponent<Animator>();
        _image = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        _image.materialForRendering.SetFloat(_circleSizeId, circleSize);
    }
}
