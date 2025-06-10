using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleManager : MonoBehaviour
{
    [Header("Referências")]
    public Transform panelPai;
    public GameObject painelVitoria;
    public Button botaoJogarNovamente;
    public Button botaoVerReplay;
    public Button botaoCancelarReplay;

    [Header("Estado")]
    private PuzzlePiece primeiraPecaSelecionada = null;
    private bool podeInteragir = true;
    private bool cancelarReplay = false;

    [Header("Histórico de Jogadas")]
    private Stack<ICommand> historicoComandos = new Stack<ICommand>();
    private List<ICommand> comandosExecutados = new List<ICommand>();
    private List<int> ordemInicialPecas = new List<int>();

    void Start()
    {
        EmbaralharPecas();
        SalvarOrdemInicial();

        botaoJogarNovamente.onClick.AddListener(JogarNovamente);
        botaoVerReplay.onClick.AddListener(() => StartCoroutine(FazerReplay()));
        botaoCancelarReplay.onClick.AddListener(CancelarReplay);
        botaoCancelarReplay.gameObject.SetActive(false);

        painelVitoria.SetActive(false);
    }

    public void PecaClicada(PuzzlePiece pecaClicada)
    {
        if (!podeInteragir) return;

        if (primeiraPecaSelecionada == null)
        {
            primeiraPecaSelecionada = pecaClicada;
            primeiraPecaSelecionada.Destacar(true);
        }
        else
        {
            if (pecaClicada == primeiraPecaSelecionada)
            {
                primeiraPecaSelecionada.Destacar(false);
                primeiraPecaSelecionada = null;
                return;
            }

            TrocarPecas(primeiraPecaSelecionada, pecaClicada);
            primeiraPecaSelecionada.Destacar(false);
            primeiraPecaSelecionada = null;
        }
    }

    void TrocarPecas(PuzzlePiece peca1, PuzzlePiece peca2)
    {
        var comando = new TrocarPecaCommand(peca1, peca2);
        comando.Executar();
        historicoComandos.Push(comando);
        comandosExecutados.Add(comando);

        VerificarSePuzzleCompleto();
    }

    public void DesfazerUltimaJogada()
    {
        if (!podeInteragir) return;

        if (historicoComandos.Count > 0)
        {
            var comando = historicoComandos.Pop();
            comando.Desfazer();
            
            // Remove o último comando da lista de executados
            if (comandosExecutados.Count > 0)
            {
                comandosExecutados.RemoveAt(comandosExecutados.Count - 1);
            }
        }
        else
        {
            Debug.Log("Nenhuma jogada para desfazer.");
        }
    }

    void EmbaralharPecas()
    {
        List<Transform> pecas = new List<Transform>();

        foreach (Transform peca in panelPai)
        {
            pecas.Add(peca);
        }

        for (int i = 0; i < pecas.Count; i++)
        {
            Transform temp = pecas[i];
            int randomIndex = Random.Range(i, pecas.Count);
            pecas[i] = pecas[randomIndex];
            pecas[randomIndex] = temp;
        }

        for (int i = 0; i < pecas.Count; i++)
        {
            pecas[i].SetSiblingIndex(i);
        }
        
        // Atualiza a ordem inicial após embaralhar
        SalvarOrdemInicial();
    }

    void SalvarOrdemInicial()
    {
        ordemInicialPecas.Clear();

        foreach (Transform peca in panelPai)
        {
            PuzzlePiece puzzlePiece = peca.GetComponent<PuzzlePiece>();
            ordemInicialPecas.Add(puzzlePiece.indiceCorreto);
        }
    }

    void RestaurarOrdemInicial()
    {
        List<Transform> pecasAtuais = new List<Transform>();

        foreach (Transform peca in panelPai)
        {
            pecasAtuais.Add(peca);
        }

        // Ordena as peças de acordo com a ordem inicial salva
        pecasAtuais.Sort((a, b) => {
            int indexA = ordemInicialPecas.IndexOf(a.GetComponent<PuzzlePiece>().indiceCorreto);
            int indexB = ordemInicialPecas.IndexOf(b.GetComponent<PuzzlePiece>().indiceCorreto);
            return indexA.CompareTo(indexB);
        });

        for (int i = 0; i < pecasAtuais.Count; i++)
        {
            pecasAtuais[i].SetSiblingIndex(i);
        }
    }

    public void VerificarSePuzzleCompleto()
    {
        for (int i = 0; i < panelPai.childCount; i++)
        {
            var peca = panelPai.GetChild(i).GetComponent<PuzzlePiece>();
            if (peca == null) continue;

            if (peca.indiceCorreto != i)
            {
                return;
            }
        }

        MostrarTelaDeVitoria();
    }

    void MostrarTelaDeVitoria()
    {
        painelVitoria.SetActive(true);
        Debug.Log("🎉 Puzzle completo! Parabéns!");
    }

    IEnumerator FazerReplay()
    {
        painelVitoria.SetActive(false);
        botaoCancelarReplay.gameObject.SetActive(true);
        botaoJogarNovamente.gameObject.SetActive(false); // Esconde no início
        podeInteragir = false;
        cancelarReplay = false;

        // Restaura para o estado inicial
        RestaurarOrdemInicial();
        yield return null;

        // Executa cada comando novamente
        for (int i = 0; i < comandosExecutados.Count; i++)
        {
            if (cancelarReplay) break;
        
            comandosExecutados[i].Executar();
            yield return new WaitForSeconds(1f);
        }

        botaoCancelarReplay.gameObject.SetActive(false);
        podeInteragir = true;
    
        if (cancelarReplay)
        {
            // Coloca todas as peças na posição correta
            for (int i = 0; i < panelPai.childCount; i++)
            {
                foreach (Transform child in panelPai)
                {
                    PuzzlePiece piece = child.GetComponent<PuzzlePiece>();
                    if (piece.indiceCorreto == i)
                    {
                        child.SetSiblingIndex(i);
                        break;
                    }
                }
            }
        
            // Mostra o botão de jogar novamente SEM mostrar a tela de vitória
            botaoJogarNovamente.gameObject.SetActive(true);
            painelVitoria.SetActive(false);
        }
        else
        {
            MostrarTelaDeVitoria();
        }
    }

    void CancelarReplay()
    {
        cancelarReplay = true;
        // Garante que o botão de jogar novamente ficará visível
        botaoJogarNovamente.gameObject.SetActive(true);
        painelVitoria.SetActive(false);
    }

    void JogarNovamente()
    {
        painelVitoria.SetActive(false);
        comandosExecutados.Clear();
        historicoComandos.Clear();
        cancelarReplay = false;
        podeInteragir = true;

        EmbaralharPecas();
        SalvarOrdemInicial();
    }
}