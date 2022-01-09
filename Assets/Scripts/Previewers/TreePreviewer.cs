using UnityEngine;

public class TreePreviewer : MonoBehaviour {
    public TreeParams param;
    GameObjectMaker maker;

    void MakePreviewTree() {
        maker.Make(Vector3.zero, Vector3.one, 0, null, Color.white, Color.white, 0);
    }

    void OnValidate()
    {
        if(param)
        {
            if(maker == null) {
                maker = new GameObjectMaker(param.gobject);
            }
            param.onChange -= MakePreviewTree;
            param.onChange += MakePreviewTree;
        }
    }
}
