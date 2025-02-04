using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputTests : MonoBehaviour
{

    public void OnMove( InputValue value )
    {
        Vector3 input = value.Get<Vector2>();
        transform.position += new Vector3( input.x, 0f, input.y );
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
