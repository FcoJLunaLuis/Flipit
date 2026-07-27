# Flipit

**Un videojuego isometrico donde coleccionas fichas, las apuestas y las volteas.**

> Hackathon Kiro 2026 — Categoria Videojuegos

---

<!-- 
  TODO: Agregar GIF o screenshot del gameplay aqui.
  Ejemplo: ![Gameplay](docs/gameplay.gif)
  Recomendacion: un GIF de 10-15 segundos mostrando exploracion + combate.
-->

## Sobre el Juego

Flipit es un videojuego isometrico en tercera persona donde un niño recorre su ciudad desde la escuela hasta su casa. En el camino puede hablar con NPCs, comprar en tiendas, coleccionar fichas (flipits) y apostarlas en combates contra retadores callejeros.

El mundo es persistente y cada dia trae pequenos cambios que mantienen la experiencia fresca.

## Features

- **Ciudad Procedural** — Generacion por etapas: grid, geometria, landmarks, NPCs, props y encuentros. (No presente en la demo)
- **Sistema de Dialogo** — Conversaciones con branching, efecto typewriter y opciones dinamicas.
- **Combate de Fichas** — Apuesta flipits y volteralas: cara o cruz decide el ganador.
- **Tienda** — Compra flipits, consumibles y mejoras de NPCs vendedores.
- **Album de Coleccion** — Registra y visualiza todas las fichas que has conseguido.
- **Ciclo Diario** — Cada dia comienza en la escuela y termina en casa; eventos aleatorios entre dias. (No disponible en demo)
- **Encuentros Forzados** — Zonas ocultas de trigger que inician combates sorpresa. (No Disponible en demo)
- **Camara Isometrica** — Con transparencia automatica de edificios cuando bloquean al jugador.
- **Sistema de Pausa** — Menu de pausa completo.
- **Sistema de Guardado** — Persistencia de progreso.

## Controles

| Accion | Tecla |
|--------|-------|
| Moverse | `WASD` |
| Interactuar | `E` |
| Pausar | `ESC` |
| **Combate** | |
| Apuntar | `Mouse` |
| Confirmar | `Space` | `Mouse` |
| **Tienda** | |
| Navegar | `WASD` |
| Comprar | `Z` |
| Salir | `X` |

## Tech Stack

| Tecnologia | Version | Uso |
|-----------|---------|-----|
| Unity | 6000.5.4f1 (Unity 6) | Motor de juego |
| C# | 9.0 / .NET Standard 2.1 | Lenguaje |
| URP | 17.0 | Render Pipeline |
| New Input System | 1.19.0 | Input |
| AI Navigation | 2.0.13 | NavMesh |
| TextMeshPro | (bundled) | UI Text |
| Timeline | 1.8.12 | Secuencias |

## Herramientas y Proceso

| Herramienta | Uso |
|-------------|-----|
| Git / GitHub | Control de versiones y colaboracion |
| Kiro | Asistente de IA para desarrollo |
| Trello | Registro de actividades y gestion de tareas |
| Obsidian | Diseno del juego (Digital Garden) |

**Enlaces:**
- [Digital Garden (Obsidian)](<https://flipitnotas.vercel.app/)

## Creditos de Assets

| Asset | Autor | Fuente |
|-------|-------|--------|
| Musica | Flowerhead | [itch.io](https://flowerheadmusic.itch.io/) |

## Arquitectura

El proyecto esta organizado en assemblies independientes:

```
Flipit.Dialogue    (base — sistema de dialogo)
      |
Flipit.Combat      (combate de fichas, depende de Dialogue)
      |
Flipit.CityTerrain (generacion de ciudad, depende de ambos)
```


**Modulos adicionales:** Core, Data, Shop, Album, NPC, DailyCycle, CoinFlip, Save, Pause, UI

Patrones utilizados: Singleton, State Machine, Event Bus, ScriptableObject data-driven, Interface abstraction.

## Como Ejecutar

1. Ir a relases y descargar la version mas actual


## Equipo

| Miembro | Rol |
|---------|-----|
| **Herbert Martell Puente** | Desarrollador |
| **Francisco Javier Luna Luis** | Desarrollador |

---

Desarrollado para el **Hackathon Kiro 2026** — Categoria Videojuegos
