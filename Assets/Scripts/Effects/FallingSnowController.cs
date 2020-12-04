using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingSnowController : MonoBehaviour
{
    public float timeForSnowToSpeedAtStart = 0.12f;
    public List<ParticleSystem> snowSystems;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SnowStartSuperFast());
    }

    private IEnumerator SnowStartSuperFast()
    {
        foreach (ParticleSystem a in snowSystems)
        {
            var main = a.main;
            main.simulationSpeed = 999999999;
        }
        yield return new WaitForSecondsRealtime(timeForSnowToSpeedAtStart);
        foreach (ParticleSystem a in snowSystems)
        {
            var main = a.main;
            main.simulationSpeed = 1;
        }
    }
}
