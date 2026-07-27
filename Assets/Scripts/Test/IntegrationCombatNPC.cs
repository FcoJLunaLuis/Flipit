using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// NPC simple para la escena de integración.
/// El jugador se acerca y presiona E para iniciar combate.
/// Genera fichas mock para el NPC.
/// </summary>
public class IntegrationCombatNPC : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private float _radioInteraccion = 3f;
    [SerializeField] private int _cantidadFichasNPC = 3;
    [SerializeField] private Transform _player;

    [Header("Visual Feedback")]
    [SerializeField] private GameObject _indicadorInteraccion;

    private List<FichaData> _fichasNPC;
    private bool _jugadorEnRango;
    private bool _combateActivo;

    private void Start()
    {
        GenerarFichasNPC();

        if (_indicadorInteraccion != null)
            _indicadorInteraccion.SetActive(false);

        if (_player == null)
        {
            var playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO != null) _player = playerGO.transform;
        }

        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.OnCombateTerminado += OnCombateTerminado;
        }

        Debug.Log($"[IntegrationCombatNPC] Listo. Acércate y presiona E para combatir. Fichas NPC: {_fichasNPC.Count}");
    }

    private void Update()
    {
        if (_player == null || _combateActivo) return;

        float distancia = Vector3.Distance(transform.position, _player.position);
        _jugadorEnRango = distancia <= _radioInteraccion;

        if (_indicadorInteraccion != null)
            _indicadorInteraccion.SetActive(_jugadorEnRango);

        if (_jugadorEnRango && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            IniciarCombate();
        }
    }

    private void IniciarCombate()
    {
        if (CombatManager.Instance == null)
        {
            Debug.LogError("[IntegrationCombatNPC] CombatManager.Instance es null.");
            return;
        }

        _combateActivo = true;
        Debug.Log("[IntegrationCombatNPC] ¡Iniciando combate!");
        CombatManager.Instance.IniciarCombate(_fichasNPC);
    }

    private void OnCombateTerminado(CombatData data)
    {
        _combateActivo = false;
        Debug.Log($"[IntegrationCombatNPC] Combate terminado. Jugador ganó {data.FichasGanadasJugador.Count}, NPC ganó {data.FichasGanadasNPC.Count}");

        // Regenerar fichas NPC para siguiente combate
        GenerarFichasNPC();
    }

    private void GenerarFichasNPC()
    {
        _fichasNPC = new List<FichaData>();
        string[] nombres = { "Escorpión Rojo", "Murciélago Oscuro", "Araña Venenosa", "Rata Nocturna", "Gato Sombra" };

        for (int i = 0; i < _cantidadFichasNPC && i < nombres.Length; i++)
        {
            _fichasNPC.Add(new FichaData
            {
                templateId = 200 + i,
                nombre = nombres[i],
                rareza = (Rareza)(i % 3),
                rango = 1,
                experienciaDeRango = 0,
                estaRoto = false,
                desgaste = Random.Range(0f, 20f),
                perk = "Ninguno",
                peso = Random.Range(8f, 18f),
                suerte = Random.Range(0.2f, 0.8f)
            });
        }
    }

    private void OnDestroy()
    {
        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.OnCombateTerminado -= OnCombateTerminado;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _radioInteraccion);
    }
}
