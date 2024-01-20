using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectRenderer : MonoBehaviour
{
    // Start is called before the first frame update
    private GameObject TapEffect;
    private GameObject Particle;
    public float duration
    {
        get { return TapEffect.GetComponent<ParticleSystem>().main.startLifetime.constant; }
    }
    public Color color
    {
        get
        { 
            return TapEffect.GetComponent<ParticleSystem>().main.startColor.color; 
        }
        set
        {
            ParticleSystem.MainModule mainmodule = TapEffect.GetComponent<ParticleSystem>().main;
            mainmodule.startColor = new ParticleSystem.MinMaxGradient(value);
            ParticleSystem.MainModule submodule = Particle.GetComponent<ParticleSystem>().main;
            submodule.startColor = new ParticleSystem.MinMaxGradient(value);
        }
    }
    public Vector3 position
    {
        get { return TapEffect.transform.position; }
        set { TapEffect.transform.position = value; }
    }
    public void init(Color _color, Vector3 _position)
    {
        getGameObject();
        color = _color;
        position = _position;
    }
    private void getGameObject()
    {
        TapEffect = this.transform.gameObject;
        Particle = transform.Find("particle").gameObject;
    }
}
