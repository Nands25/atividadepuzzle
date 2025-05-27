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
    private List<int> ordemInicialPecas = new List<int>(); // Ordem embaralhada salva

    void Start()
    {
        EmbaralharPecas();
        SalvarOrdemInicial();

        // Eventos dos botões
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

        for (int i = 0; i < ordemInicialPecas.Count; i++)
        {
            int indiceBuscado = ordemInicialPecas[i];
            Transform pecaEncontrada = pecasAtuais.Find(p =>
                p.GetComponent<PuzzlePiece>().indiceCorreto == indiceBuscado
            );

            if (pecaEncontrada != null)
            {
                pecaEncontrada.SetSiblingIndex(i);
            }
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
        podeInteragir = false;
        cancelarReplay = false;

        RestaurarOrdemInicial();
        yield return null;

        for (int i = 0; i < comandosExecutados.Count; i++)
        {
            comandosExecutados[i].Executar();

            if (cancelarReplay)
            {
                // Executa tudo que falta de forma instantânea
                for (int j = i + 1; j < comandosExecutados.Count; j++)
                {
                    comandosExecutados[j].Executar();
                }
                break;
            }
            else
            {
                yield return new WaitForSeconds(1f);
            }
        }

        botaoCancelarReplay.gameObject.SetActive(false);
        podeInteragir = true;
        MostrarTelaDeVitoria();
    }

    void CancelarReplay()
    {
        cancelarReplay = true;
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
