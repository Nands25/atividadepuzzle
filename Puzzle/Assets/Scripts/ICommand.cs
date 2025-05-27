using UnityEngine;

public interface ICommand
{
    void Executar();
    void Desfazer();
}
