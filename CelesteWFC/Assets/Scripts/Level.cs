using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class Level : MonoBehaviour
{
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private Transform madeline;

    private void Start() {
        if (Container.I == null || Container.I.wfc == null) {
            Debug.LogError("Level contains no WFC!");
            return;
        }

        var wfc = Container.I.wfc;
        for (var y = 0; y < wfc.height; ++y) {
            for (var x = 0; x < wfc.width; ++x) {
                var state = wfc.grid[y, x].states[0];
                var position = new Vector3Int(x, wfc.height - 1 - y, 0);

                tilemap.SetTile(position, state.tile);

                var angle = -90f * state.timesRotatedClockwise;
                var rotMat = Matrix4x4.Rotate(Quaternion.Euler(0f, 0f, angle));

                tilemap.SetTransformMatrix(position, rotMat);
            }
        }
    }

    private void Update() {
        if (madeline.position.y < -10f) {
            madeline.position = new Vector3(1.5f, 1.8f, 0f);
        }
    }

    public static void LoadEditor() {
        SceneManager.LoadScene("Editor");
    }
}