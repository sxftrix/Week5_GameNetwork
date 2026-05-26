using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textMesh;
    private float moveSpeed = 1.5f;
    private float lifetime = 1f;

    public void Setup(int damageAmount)
    {
        textMesh.SetText(damageAmount.ToString());
    }

    void Update()
    {
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;

        if (Camera.main != null)
        {
            transform.LookAt(transform.position + Camera.main.transform.rotation * Vector3.forward, Camera.main.transform.rotation * Vector3.up);
        }

        lifetime -= Time.deltaTime;
        if (lifetime <= 0) 
        {
            Destroy(gameObject);
        }
    }
}