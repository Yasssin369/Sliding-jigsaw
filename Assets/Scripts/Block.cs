using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    public event System.Action<Block> OnBlockPressed;
    public event System.Action OnFinishedMoving;

    public Vector2Int coord;
    public Vector2Int startingCoord;
    public void Init(Texture2D image,Vector2Int startingCoord)
    {
        this.startingCoord = startingCoord;
        coord = startingCoord;
        GetComponent<MeshRenderer>().material.shader = Shader.Find("Unlit/Texture");
        GetComponent<MeshRenderer>().material.mainTexture = image;
    }
    private void OnMouseDown()
    {
        if(OnBlockPressed!=null)
        {
            OnBlockPressed(this);
        }
    }
    public void MoveToPosition(Vector2 target, float duration)
    {
        StartCoroutine(AnimateMove(target, duration));
    }
    IEnumerator AnimateMove(Vector2 target, float duration)
    {
        Vector2 initialPos = transform.position;
        float percent = 0;
        while (percent<1)
        {
            percent += Time.deltaTime/duration;
            transform.position = Vector2.Lerp(initialPos, target, percent);
            yield return null;
        }
        if(OnFinishedMoving != null)
        {
            OnFinishedMoving();
        }
    }
    public bool IsAtStartingCoord()
    {
        return coord == startingCoord;
    }
}
