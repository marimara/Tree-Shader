using UnityEngine;

public class SunRotationSimulator : MonoBehaviour
{
    [Header("Day Simulation")]
    [Tooltip("Duração de um dia completo, em segundos.")]
    [Min(1f)]
    public float dayDuration = 30f;

    [Tooltip("Ângulo inicial do sol.")]
    [Range(0f, 360f)]
    public float startAngle = 0f;

    [Tooltip("Pausa a simulação.")]
    public bool paused = false;

    private float time;

    private void Start()
    {
        time = (startAngle / 360f) * dayDuration;
    }

    private void Update()
    {
        if (paused)
            return;

        time += Time.deltaTime;

        float normalizedTime = (time % dayDuration) / dayDuration;
        float angle = normalizedTime * 360f;

        transform.rotation = Quaternion.Euler(angle, -30f, 0f);
    }
}