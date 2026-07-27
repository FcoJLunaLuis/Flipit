# Requirements Document

## Introduction

Este documento define los requisitos para conectar el sistema de Álbum (colección de fichas del jugador basada en ScriptableObjects) con el sistema de Combate/Volteo (combate de torre con físicas). La integración permite que el jugador seleccione fichas de su álbum para apostar en combate, que las fichas apostadas se gestionen correctamente durante el combate, y que los resultados (fichas ganadas/perdidas) se reflejen permanentemente en los álbumes de ambos participantes.

## Glossary

- **Album_Data**: Clase que gestiona la colección de fichas del jugador con operaciones de agregar, remover, consultar y paginar.
- **Album_Manager**: Singleton MonoBehaviour que coordina AlbumData, AlbumUI e InputHandler. Punto de acceso público al álbum.
- **Combat_Manager**: Singleton MonoBehaviour que orquesta el flujo completo de combate: BetSelection → CoinFlip → ThrowTurns → Summary.
- **Combat_Data**: Clase que almacena el estado completo de un combate en curso, incluyendo fichas apostadas, torre, turnos y resultados.
- **Bet_Selection_Logic**: Clase que valida y gestiona la selección de fichas para apostar (máximo 5) y la ficha lanzadora.
- **Ficha_Data**: Instancia runtime mutable de una ficha con atributos como peso, suerte, rareza y estado.
- **Ficha_Template**: ScriptableObject inmutable que define los atributos base de un tipo de ficha.
- **Ficha_Lanzadora**: Ficha seleccionada por el jugador para realizar el lanzamiento físico. No forma parte de la apuesta y no se arriesga.
- **Torre**: Estructura lógica y física formada al apilar las fichas apostadas por ambos participantes.
- **Volteo**: Acción de lanzar la ficha lanzadora contra la torre para voltear fichas. Las fichas volteadas en un turno son ganadas por quien lanzó.
- **Combat_Album_Bridge**: Componente puente que conecta el álbum del jugador con el sistema de combate, gestionando transferencias de fichas.
- **NPC_Pool**: Conjunto de fichas disponibles del NPC oponente para generar su apuesta.
- **Fichas_En_Juego**: Fichas que han sido removidas temporalmente del álbum mientras participan en un combate activo.

## Requirements

### Requirement 1: Provisión de Fichas del Álbum para la Selección de Apuesta

**User Story:** Como jugador, quiero que la pantalla de selección de apuesta muestre solamente las fichas que poseo en mi álbum, para poder apostar únicamente fichas que realmente tengo.

#### Acceptance Criteria

1. WHEN el Combat_Manager inicia la fase BetSelection, THE Combat_Album_Bridge SHALL obtener la lista de fichas disponibles del jugador desde Album_Data utilizando ObtenerTodasLasFichas y proporcionar el resultado a Bet_Selection_Logic en un máximo de 1 frame.
2. THE Combat_Album_Bridge SHALL filtrar las fichas disponibles excluyendo aquellas con el atributo estaRoto en valor verdadero y aquellas con cantidad menor o igual a cero.
3. IF el jugador posee menos de (CombatConfig.minFichasApuesta + 1) fichas únicas no rotas en su álbum, THEN THE Combat_Album_Bridge SHALL impedir el inicio del combate y notificar al jugador mediante un mensaje en pantalla indicando que no posee fichas suficientes para apostar.
4. THE Combat_Album_Bridge SHALL proporcionar a Bet_Selection_Logic únicamente instancias de Ficha_Data que correspondan a fichas presentes en el álbum del jugador con cantidad mayor o igual a 1.
5. IF Album_Data retorna una lista nula o la invocación de ObtenerTodasLasFichas falla, THEN THE Combat_Album_Bridge SHALL impedir el inicio del combate y registrar el error mediante Debug.LogError con el prefijo [Combat_Album_Bridge].

### Requirement 2: Selección de Fichas para Apuesta

**User Story:** Como jugador, quiero seleccionar hasta 5 fichas de mi álbum para apostar y una ficha lanzadora separada, para participar en el combate de volteo.

#### Acceptance Criteria

1. WHILE la fase de combate es BetSelection, THE Bet_Selection_Logic SHALL permitir al jugador seleccionar entre minFichasApuesta y maxFichasApuesta fichas para apostar, donde los límites son configurables mediante CombatConfig (por defecto mínimo 1, máximo 5).
2. WHILE la fase de combate es BetSelection, THE Bet_Selection_Logic SHALL permitir al jugador seleccionar exactamente una Ficha_Lanzadora que no forme parte de las fichas apostadas y que no tenga el estado estaRoto en true.
3. WHEN el jugador intenta seleccionar una ficha que ya tiene cantidad cero en el álbum, THE Bet_Selection_Logic SHALL rechazar la selección sin agregarla a la lista de fichas apostadas ni como ficha lanzadora.
4. WHEN el jugador intenta seleccionar para apostar una ficha con el mismo templateId que una ya seleccionada, THE Bet_Selection_Logic SHALL rechazar la selección y mantener la lista de apuesta sin cambios.
5. WHEN el jugador confirma la apuesta y la validación falla, THE Bet_Selection_Logic SHALL retornar false e indicar mediante un mensaje de error la razón del fallo (fichas insuficientes, lanzadora no seleccionada, o lanzadora duplicada en apuesta).
6. WHEN el jugador intenta seleccionar una ficha con estaRoto en true como ficha de apuesta, THE Bet_Selection_Logic SHALL rechazar la selección sin agregarla a la lista.
7. WHEN el jugador confirma la apuesta y la validación es exitosa, THE Bet_Selection_Logic SHALL retornar true con mensaje de error vacío, confirmando que existe al menos minFichasApuesta ficha(s) apostada(s) y exactamente 1 Ficha_Lanzadora válida.

### Requirement 3: Remoción Temporal de Fichas durante el Combate

**User Story:** Como jugador, quiero que las fichas que apuesto se marquen como en uso durante el combate, para que no pueda apostarlas en otro combate simultáneamente ni que desaparezcan del sistema.

#### Acceptance Criteria

1. WHEN el jugador confirma su apuesta, THE Combat_Album_Bridge SHALL remover las fichas apostadas del Album_Data del jugador invocando RemoverFichas por cada templateId y cantidad correspondiente.
2. WHEN el jugador confirma su apuesta, THE Combat_Album_Bridge SHALL almacenar las Fichas_En_Juego (templateId y cantidad de cada ficha apostada, más el templateId de la Ficha_Lanzadora) en un registro persistente a disco, de modo que la información sobreviva un cierre inesperado de la aplicación.
3. IF RemoverFichas retorna false para alguna ficha durante la confirmación de apuesta, THEN THE Combat_Album_Bridge SHALL cancelar la operación completa, restaurar al Album_Data cualquier ficha ya removida en esa misma operación, y emitir un mensaje de error indicando insuficiencia de fichas.
4. IF el combate se interrumpe antes de la fase Summary por cualquiera de las siguientes causas: cierre forzado de la aplicación, descarga de la escena de combate sin alcanzar Summary, o invocación de Application.Quit, THEN THE Combat_Album_Bridge SHALL restaurar todas las Fichas_En_Juego (apostadas y lanzadora) al Album_Data del jugador mediante AgregarFicha al siguiente inicio de la escena de combate o del AlbumManager.
5. WHEN el jugador confirma su apuesta, THE Combat_Album_Bridge SHALL remover la Ficha_Lanzadora del Album_Data del jugador e incluirla en el registro de Fichas_En_Juego.
6. WHEN la fase de combate alcanza Summary, THE Combat_Album_Bridge SHALL eliminar el registro persistente de Fichas_En_Juego y restaurar la Ficha_Lanzadora al Album_Data del jugador mediante AgregarFicha.

### Requirement 4: Generación de Apuesta del NPC

**User Story:** Como jugador, quiero que el NPC apueste una cantidad equivalente de fichas de su pool, para que el combate sea justo.

#### Acceptance Criteria

1. WHEN el jugador confirma su apuesta con una cantidad menor al máximo configurado (maxFichasApuesta), THE NPC_Bet_Generator SHALL generar una apuesta del NPC con una cantidad de fichas igual a la del jugador ±1 (variación aleatoria), clampeada entre 1 y maxFichasApuesta, sin exceder la cantidad de fichas no rotas disponibles en el NPC_Pool.
2. WHEN el jugador confirma su apuesta con una cantidad igual al máximo configurado (maxFichasApuesta), THE NPC_Bet_Generator SHALL generar una apuesta del NPC con exactamente maxFichasApuesta fichas.
3. THE NPC_Bet_Generator SHALL seleccionar fichas del NPC_Pool proporcionado al iniciar el combate, excluyendo fichas cuyo atributo estaRoto sea true, en orden aleatorio.
4. THE NPC_Bet_Generator SHALL seleccionar una Ficha_Lanzadora para el NPC de forma aleatoria entre las fichas no rotas del NPC_Pool que no formen parte de las fichas apostadas del NPC (comparando por templateId).
5. IF el NPC_Pool no contiene fichas válidas (lista vacía o todas con estaRoto == true) al momento de generar la apuesta, THEN THE NPC_Bet_Generator SHALL retornar una lista vacía y registrar una advertencia en el log.
6. IF no existen fichas válidas disponibles para Ficha_Lanzadora después de excluir las fichas apostadas y las rotas, THEN THE NPC_Bet_Generator SHALL retornar null como Ficha_Lanzadora y registrar una advertencia en el log.

### Requirement 5: Construcción de la Torre

**User Story:** Como jugador, quiero que las fichas apostadas por ambos participantes se apilen en una torre, para que el volteo funcione con las fichas reales de la apuesta.

#### Acceptance Criteria

1. WHEN ambas apuestas están confirmadas, THE Tower_Builder SHALL construir la torre lógica mezclando aleatoriamente las fichas apostadas del jugador y las fichas apostadas del NPC, generando una lista de TowerSlots cuyo tamaño total es igual a la suma de fichas apostadas por ambos participantes.
2. WHEN la torre lógica está construida, THE Tower_Physics_Builder SHALL instanciar un GameObject con Rigidbody por cada TowerSlot, apilados verticalmente en orden ascendente, con isKinematic activado inicialmente.
3. THE Tower_Builder SHALL asignar a cada TowerSlot la Ficha_Data original, su propietario (Jugador o NPC) mediante SlotOwner, una posición lógica (0, índice) y un estado inicial de BocaArriba.
4. IF la lista de fichas apostadas del jugador o del NPC está vacía o es nula, THEN THE Tower_Builder SHALL retornar una lista vacía de TowerSlots sin generar la torre física.

### Requirement 6: Resolución de Volteo y Asignación de Fichas Ganadas

**User Story:** Como jugador, quiero que las fichas que volteo en mi turno se me otorguen, para que gane fichas del oponente.

#### Acceptance Criteria

1. WHEN el FlipDetector completa la evaluación de volteo (OnEvaluacionCompleta) y al menos un TowerSlot transiciona a EstaVolteada == true durante el lanzamiento actual, THE CombatData SHALL agregar la FichaData de cada slot volteado a la lista FichasGanadasJugador si TurnoActual es Jugador, o a FichasGanadasNPC si TurnoActual es NPC.
2. THE CombatData SHALL asignar fichas volteadas al jugador del turno actual independientemente del SlotOwner (Dueno) original del TowerSlot volteado.
3. WHEN todas las fichas de la torre cumplen EstaVolteada == true (TodasLasFichasVolteadas retorna true), THE CombatManager SHALL avanzar la fase a CombatPhase.Summary e invocar OnCombateTerminado con el CombatData resultante.
4. IF un lanzamiento no produce ningún TowerSlot volteado (FichasVolteadas.Count == 0), THEN THE CombatManager SHALL registrar el lanzamiento en el historial, cambiar el turno al oponente y continuar con el siguiente turno de lanzamiento.

### Requirement 7: Transferencia de Fichas Post-Combate al Álbum del Jugador

**User Story:** Como jugador, quiero que las fichas que gané en combate se agreguen permanentemente a mi álbum, para que mi colección crezca con las victorias.

#### Acceptance Criteria

1. WHEN el CombatManager dispara OnCombateTerminado con la fase Summary, THE Combat_Album_Bridge SHALL invocar AlbumManager.AgregarFicha por cada ficha contenida en CombatData.FichasGanadasJugador, incluyendo el caso en que la lista esté vacía (sin errores ni invocaciones).
2. WHEN la fase de combate es Summary, THE Combat_Album_Bridge SHALL verificar que las fichas perdidas por el jugador (fichas en CombatData.FichasGanadasNPC que pertenecían a FichasApostadasJugador) permanecen removidas del Album_Data sin ejecutar una segunda remoción.
3. WHEN la fase de combate es Summary, THE Combat_Album_Bridge SHALL invocar AlbumManager.AgregarFicha con CombatData.FichaLanzadoraJugador para restaurar la Ficha_Lanzadora al álbum del jugador, independientemente del resultado del combate.
4. WHEN las operaciones de los criterios 1, 2 y 3 se han ejecutado completamente, THE Combat_Album_Bridge SHALL disparar un evento OnAlbumActualizado con el CombatData asociado para notificar a los suscriptores de la actualización del álbum.
5. IF CombatData.FichasGanadasJugador o CombatData.FichaLanzadoraJugador es null al momento de la transferencia, THEN THE Combat_Album_Bridge SHALL omitir la operación correspondiente, registrar un mensaje de advertencia con prefijo [Combat_Album_Bridge], y continuar procesando las operaciones restantes sin interrumpir el flujo.

### Requirement 8: Gestión de Fichas Perdidas

**User Story:** Como jugador, quiero que las fichas que pierdo en combate desaparezcan permanentemente de mi álbum, para que el riesgo de apostar sea real.

#### Acceptance Criteria

1. WHEN la fase de combate es Summary, THE Combat_Album_Bridge SHALL identificar las fichas perdidas por el jugador filtrando FichasGanadasNPC para incluir únicamente aquellas fichas cuyo propietario original es el jugador (presentes en FichasApostadasJugador).
2. WHEN la fase de combate es Summary, THE Combat_Album_Bridge SHALL verificar que cada ficha perdida identificada ya fue removida del Album_Data durante la fase de remoción temporal (Requirement 3) y no ejecutar una segunda operación de remoción sobre el álbum.
3. WHEN la fase de combate es Summary, THE Combat_Album_Bridge SHALL eliminar las fichas perdidas de la lista interna Fichas_En_Juego para que no sean restauradas al álbum al finalizar el combate.
4. IF FichasGanadasNPC contiene todas las fichas de FichasApostadasJugador (pérdida total), THEN THE Combat_Album_Bridge SHALL procesar la pérdida completa sin errores y sin intentar restaurar ninguna ficha apostada al álbum.
5. WHEN la fase de combate es Summary, THE Combat_Album_Bridge SHALL descartar de FichasGanadasNPC aquellas fichas cuyo propietario original es el NPC (presentes en FichasApostadasNPC), ya que estas retornan a su pool y no afectan el álbum del jugador.

### Requirement 9: Persistencia del Estado del Álbum Post-Combate

**User Story:** Como jugador, quiero que los cambios en mi álbum después de un combate se guarden, para que no pierda mi progreso.

#### Acceptance Criteria

1. WHEN la transferencia de fichas post-combate se completa (todas las fichas ganadas o perdidas han sido aplicadas al Album_Data), THE Album_Manager SHALL invocar GuardarDatos() para persistir el estado actualizado mediante SaveSystem.Guardar(SaveData) dentro de los 2 segundos siguientes a la finalización de la transferencia.
2. WHEN GuardarDatos() se ejecuta post-combate, THE Album_Manager SHALL crear un SaveData con AlbumSaveData.CrearDesdeAlbum(albumData) que refleje el estado actual del álbum incluyendo las fichas transferidas durante el combate.
3. IF SaveSystem.Guardar() retorna false, THEN THE Album_Manager SHALL registrar un error mediante Debug.LogError con prefijo "[AlbumManager]" y mantener el Album_Data actualizado en memoria sin revertir los cambios, permitiendo hasta 3 reintentos automáticos con un intervalo de 1 segundo entre cada intento.
4. IF todos los reintentos de guardado fallan (3 intentos fallidos), THEN THE Album_Manager SHALL mantener el estado en memoria para que sea persistido en el próximo evento de guardado (cierre de álbum, OnApplicationQuit, o siguiente combate) y registrar una advertencia mediante Debug.LogWarning indicando que el guardado queda pendiente.
5. WHEN el guardado post-combate se ejecuta exitosamente (SaveSystem.Guardar() retorna true), THE Album_Manager SHALL registrar un mensaje de confirmación mediante Debug.Log con prefijo "[AlbumManager]" indicando el número de fichas únicas persistidas.

### Requirement 10: Preservación del Flujo de Combate Existente

**User Story:** Como desarrollador, quiero que la integración con el álbum no modifique el flujo existente de combate (rondas, turnos, físicas), para mantener la estabilidad del sistema.

#### Acceptance Criteria

1. THE Combat_Album_Bridge SHALL operar como componente externo al Combat_Manager, suscribiéndose a los eventos existentes (OnCombateIniciado, OnCombateTerminado, OnFaseCambiada) sin ser referenciado desde Combat_Manager mediante [SerializeField] ni inyección directa.
2. THE Combat_Album_Bridge SHALL utilizar la interfaz pública existente de Album_Manager (AgregarFicha, ObtenerAlbumData) sin agregar, remover ni modificar métodos, campos o lógica en Album_Manager.cs.
3. THE Combat_Manager SHALL mantener su flujo de fases (BetSelection → CoinFlip → ThrowTurn → ResolveThrow → CheckEnd → Summary) sin modificaciones en CombatManager.cs para la integración del álbum.
4. THE Combat_Album_Bridge SHALL utilizar la interfaz existente de AlbumChipInventory (GetOwnedChipIds, RemoveChips, AddChips) para todas las operaciones de remoción y adición de fichas durante el combate.
5. IF el Combat_Album_Bridge genera una excepción durante el procesamiento de un evento de combate, THEN THE Combat_Album_Bridge SHALL capturar la excepción internamente y registrar el error sin interrumpir ni alterar la ejecución del Combat_Manager.

### Requirement 11: Validación Pre-Combate

**User Story:** Como jugador, quiero recibir feedback claro si no puedo iniciar un combate por falta de fichas, para entender qué necesito.

#### Acceptance Criteria

1. WHEN el jugador interactúa con un FlipCombat_NPC, RandomEncounter_NPC, o TriggerZone_Encounter para iniciar un combate, THE Combat_Album_Bridge SHALL ejecutar la validación pre-combate antes de invocar CombatManager.IniciarCombate(), contando únicamente fichas con estaRoto == false desde AlbumData.ObtenerTodasLasFichas().
2. IF el número total de fichas no rotas del jugador es menor que CombatConfig.minFichasApuesta + 1, THEN THE Combat_Album_Bridge SHALL bloquear el inicio del combate y mostrar un mensaje en la UI indicando que se requieren al menos (minFichasApuesta + 1) fichas no rotas para combatir.
3. IF todas las fichas del jugador en el álbum tienen estaRoto == true, THEN THE Combat_Album_Bridge SHALL bloquear el inicio del combate y mostrar un mensaje en la UI indicando que no posee fichas en condiciones de combate.
4. IF el jugador tiene exactamente CombatConfig.minFichasApuesta fichas no rotas (suficientes para apostar pero ninguna adicional disponible como lanzadora), THEN THE Combat_Album_Bridge SHALL bloquear el inicio del combate y mostrar un mensaje en la UI indicando que necesita al menos una ficha no rota adicional para usar como lanzadora.
5. WHEN la validación pre-combate falla por cualquiera de las condiciones anteriores, THE Combat_Album_Bridge SHALL cancelar el flujo de combate y devolver al jugador al estado de exploración sin modificar el estado del álbum.
