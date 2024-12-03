using UnityEngine;

public class Container : MonoBehaviour
{
    public static Container I { get; private set; }
    public WaveFunctionCollapse wfc;

    private void Awake() {
        if (I == null) {
            I = this;
            DontDestroyOnLoad(gameObject);
        }
        else {
            Destroy(gameObject);
        }
    }
}