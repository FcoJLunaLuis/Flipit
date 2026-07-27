# Proyecto: Flipit
# Contexto para el Asistente de IA (Kiro)

## 1. Descripción del Proyecto
Somos un equipo de 3 desarrolladores participando en un hackathon. Estamos creando un videojuego en **Unity** con una estética única:
*   **Mundo**: Renderizado en 2D isométrico (usando Tilemaps o entorno 3D pero en isometrico).
*   **Gameplay**: Experiencia en 3D (personajes, físicas e interacciones en tres dimensiones).
*   **Plataforma**: [PC].

**Objetivo Principal**: Tener un prototipo jugable y pulido para la demo final del hackathon.

## 2. Stack Tecnológico
*   **Motor**: Unity (versión 6000.5.4f).
*   **Renderizado**: Plantilla 3D / URP (Universal Render Pipeline).
*   **Lenguaje**: C#.
*   **Control de Versiones**: Git y GitHub.
*   **Gestión de Tareas**: Trello.

## 3. Convenciones de Código y Estilo
*   **Lenguaje**: Todos los scripts serán en C#.
*   **Estilo**: Seguiremos las convenciones estándar de C# y Unity.
    *   `PascalCase` para clases y métodos públicos.
    *   `camelCase` para variables locales y parámetros.
    *   `_camelCase` con guión bajo para variables privadas de instancia.
*   **Comentarios**: Se prioriza el código autodocumentado. Los comentarios se usarán para explicar el "por qué" de una decisión compleja, no el "qué" hace el código.

## 4. Flujo de Trabajo (Git y Trello)
*   **Ramas (Git)**:
    *   `main`: La rama principal y estable. Todo lo que esté aquí debe estar listo para la demo.
    *   `feature/*`: Para desarrollar nuevas funcionalidades. Se fusionan a `main` mediante Pull Requests.
    *   `hotfix/*`: Para correcciones urgentes y críticas.
*   **Gestión de Tareas (Trello)**:
    *   Usaremos el tablero de Trello para rastrear el progreso. Las columnas son:
        1.  **Backlog**: Ideas y tareas pendientes.
        2.  **MVP (Lo Obligatorio)**: Tareas esenciales para la demo. ¡Prioridad máxima!
        3.  **En Progreso**: Tareas en las que se está trabajando activamente.
        4.  **En Revisión**: Tareas completadas que necesitan ser probadas o revisadas por otro compañero.
        5.  **Listo/Hecho**: Tareas terminadas e integradas en `main`.
*   **Regla de Oro**: Antes de empezar una nueva tarea, ¡mueve la tarjeta en Trello y crea una rama en Git!

## 5. Estructura de Carpetas del Proyecto (Unity)
Para mantener el orden, seguiremos esta estructura dentro de la carpeta `Assets/`:
*   `_Project/`: Raíz de todos los assets del proyecto.
    *   `Animations/`: Controladores y clips de animación.
    *   `Audio/`: Efectos de sonido y música.
    *   `Materials/`: Materiales y shaders.
    *   `Models/`: Modelos 3D.
    *   `Prefabs/`: Prefabs reutilizables.
    *   `Scenes/`: Escenas del juego.
    *   `Scripts/`: Todos los scripts de C#.
        *   `Gameplay/`: Lógica central del juego.
        *   `UI/`: Lógica de la interfaz de usuario.
        *   `Utils/`: Herramientas y extensiones.
    *   `Sprites/`: Texturas y sprites para el mundo 2D.
    *   `Tilemaps/`: Assets relacionados con los Tilemaps isométricos.

## 6. Principios de Diseño
*   **Jugabilidad por Encima de Todo**: La diversión y la experiencia de usuario son nuestra prioridad.
*   **Iteración Rápida**: Preferimos tener un prototipo funcional y mejorarlo, a tener un diseño perfecto en papel.
*   **Comunicación Constante**: Ante cualquier duda o bloqueo, ¡preguntar al equipo es la mejor opción!

## 7. Notas Adicionales para el Asistente (Kiro)
*   **Enfoque de las Respuestas**: Prioriza dar soluciones prácticas y código funcional sobre explicaciones teóricas extensas.
*   **Recursos**: Si necesitas buscar documentación, prioriza la Documentación Oficial de Unity y fuentes confiables como Unity Learn.
*   **Contexto del Hackathon**: Ten en cuenta que el tiempo es limitado. Ofrece soluciones que sean rápidas de implementar y que requieran el menor número de dependencias externas posible.