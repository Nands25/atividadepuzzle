using UnityEngine;

public class TrocarPecaCommand : ICommand
{
    private PuzzlePiece peca1;
    private PuzzlePiece peca2;

    public TrocarPecaCommand(PuzzlePiece p1, PuzzlePiece p2)
    {
        peca1 = p1;
        peca2 = p2;
    }

    public void Executar()
    {
        Trocar();
    }

    public void Desfazer()
    {
        Trocar(); // Trocar de novo desfaz
    }

    private void Trocar()
    {
        int index1 = peca1.transform.GetSiblingIndex();
        int index2 = peca2.transform.GetSiblingIndex();

        peca1.transform.SetSiblingIndex(index2);
        peca2.transform.SetSiblingIndex(index1);
    }
}