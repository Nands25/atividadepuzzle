using UnityEngine;
using UnityEngine.UI;

public class PuzzlePiece : MonoBehaviour
{
    public PuzzleManager puzzleManager;
    private Button button;
    private Image image;
    public int indiceCorreto;

    private void Awake()
    {
        button = GetComponent<Button>();
        image = GetComponent<Image>();

        button.onClick.AddListener(OnPieceClicked);
    }

    void OnPieceClicked()
    {
        puzzleManager.PecaClicada(this);
    }

    public void Destacar(bool destacar)
    {
        if (destacar)
            image.color = Color.yellow;
        else
            image.color = Color.white;
    }
}