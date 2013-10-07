using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;

using Engine.Events;

// CUSTOM

public enum GameZones {
    left,
    right
}

public class GameContentDisplayTypes {
    public static string gamePlayerOutOfBounds = "content-game-player-out-of-bounds";

    public static string gameChoicesOverview = "content-game-game-choices-overview";

    public static string gameChoicesItemStart = "content-game-game-choices-item-start";
    public static string gameChoicesItemResult = "content-game-game-choices-item-result";

    public static string gameCollectOverview = "content-game-game-collect-overview";

    public static string gameCollectItemStart = "content-game-game-collect-item-start";
    public static string gameCollectItemResult = "content-game-game-collect-item-result";
}

// GLOBAL

public enum GameControllerType {
    Iso2DSide,
    Iso3D,
    Iso2DTop,
    Perspective3D
}

public enum GameModeGlobal {
    GameModeArcade, // ARCADE
    GameModeTraining, // TRAINING
    GameModeChallenge // CAREER
}

public enum GameModeArcade {
    GameModeArcadeDefault
}

public enum GameModeChallenge {
    GameModeChallengeDefault
}

public enum GameModeTraining {
    GameModeTrainingChoice,
    GameModeTrainingCollect,
    GameModeTrainingContent,
    GameModeTrainingTips,
    GameModeTrainingTipsControls,
    GameModeTrainingRPGEnergy,
    GameModeTrainingRPGHealth
}

public enum GameModeTrainingChoice {
    GameModeTrainingChoiceOverview,
    GameModeTrainingChoiceDisplayItem,
    GameModeTrainingChoiceAnswer,
    GameModeTrainingChoiceResults,
}

public class GamePlayerMessages {

    public static string PlayerAnimation = "playerAnimation";
    public static string PlayerAnimationSkill = "skill";
    public static string PlayerAnimationAttack = "attack";
    public static string PlayerAnimationFall = "fall";
}

public enum GameStateGlobal {
    GameNotStarted,
    GameInit,
    GamePrepare,
    GameStarted,
    GameQuit,
    GamePause,
    GameResume,
    GameResults,
    GameContentDisplay, // dialog or in progress choice/content/collection status
}

public class GameActorItem {
    public float health = 1f;
    public float difficulty = .3f;
    public float scale = 1f;
    public float speed = 1f;
    public float attack = 1f;
    public float defense = 1f;
    
    public string characterCode = "character-enemy-goblin";
    public string prefabCode = "GameEnemyGobln";
}

public class GameMessages {
    public static string scores = "game-shooter-scores";
    public static string score = "game-shooter-score";
    public static string ammo = "game-shooter-ammo";
    public static string coin = "game-shooter-coin";
    public static string state = "game-shooter-state";
}

public class GameStatCodes {
    public static string wins = "wins";
    public static string losses = "losses";
    public static string shots = "shots";
    public static string destroyed = "destroyed";
    public static string score = "score";
}

public class GameGameRuntimeData {
    public double currentLevelTime = 0;
    public double timeRemaining = 90;
    public double coins = 0;
    public string levelCode = "";
    public double score = 0;
    
    public GameGameRuntimeData() {
        Reset();
    }
    
    public void Reset() {
        currentLevelTime = 0;
        timeRemaining = 90;
        coins = 0;
        levelCode = "";
        score = 0;
        ResetTimeDefault();
    }
    
    public bool timeExpired {
        get {
            if(timeRemaining <= 0) {
                timeRemaining = 0;
            return true;
            }
            return false;
        }
    }
    
    public void SubtractTime(double delta) {
        if(timeRemaining > 0) {
            timeRemaining -= delta;
        }
    }
    
    public void ResetTimeDefault() {
        timeRemaining = 90;
    }
    
    public void ResetTime(double timeTo) {
        timeRemaining = timeTo;
    }
    
    public void AppendTime(double timeAppend) {
        timeRemaining += timeAppend;
    }
}

public enum GameCameraView {
    ViewSide, // tecmo
    ViewSideTop, // tecmo
    ViewBackTilt, // backbreaker cam
    ViewBackTop // john elway cam
}

public enum GameRunningState {
    PAUSED,
    RUNNING,
    STOPPED
}

public class BaseGameController : MonoBehaviour {

    public GamePlayerController currentGamePlayerController;

    public Dictionary<string, GamePlayerController> gamePlayerControllers;
    public Dictionary<string, GamePlayerProjectile> gamePlayerProjectiles;
    public List<string> gameCharacterTypes = new List<string>();
    int currentCharacterTypeIndex = 0;
    //int lastCharacterTypeIndex = 0;

    public bool initialized = false;

    public bool allowedEditing = true;
    public bool isAdvancing = false;

    public GameStateGlobal gameState = GameStateGlobal.GameNotStarted;
    public GameModeGlobal gameMode = GameModeGlobal.GameModeArcade;
    
    public UnityEngine.Object prefabDraggableContainer;
    
    public GameObject levelBoundaryContainerObject;
    public GameObject levelContainerObject;
    public GameObject levelItemsContainerObject;
    public GameObject levelActorsContainerObject;
    public GameObject levelZonesContainerObject;
    public GameObject levelSpawnsContainerObject;
    public GameObject itemContainerObject;
    
    public GameObject gameContainerObject;
    
    public GameObject boundaryEdgeObjectTopRight;
    public GameObject boundaryEdgeObjectTopLeft;
    public GameObject boundaryEdgeObjectBottomRight;
    public GameObject boundaryEdgeObjectBottomLeft; 
    
    public GameObject boundaryObjectTopRight;
    public GameObject boundaryObjectTopLeft;
    public GameObject boundaryObjectBottomRight;
    public GameObject boundaryObjectBottomLeft;
    
    public GameBounds gameBounds;
    
    public GameObject boundaryTopLeft;
    public GameObject boundaryTopRight;
    public GameObject boundaryBottomLeft;
    public GameObject boundaryBottomRight;
    public GameObject boundaryTopCeiling;
    public GameObject boundaryBottomAbyss;

    public GameObject gameEndZoneLeft;
    public GameObject gameEndZoneRight;
    
    public GameGameRuntimeData runtimeData; 
    
    public Camera cameraGame;
    public Camera cameraGameGround;
    public GameCameraView cameraView = GameCameraView.ViewSide;
    public GameRunningState gameRunningState = GameRunningState.STOPPED;
    public GameControllerType gameControllerType = GameControllerType.Iso3D;
    
    float currentTimeBlock = 0.0f;
    float actionInterval = 1.3f;

    public float defaultLevelTime = 90;

    public string contentDisplayCode = "default";

    public bool isGameOver = false;
    public bool isPlayerOutOfBounds = false;
    
    // CUSTOM
    
    public GameZones currentGameZone = GameZones.right;

    // ----------------------------------------------------------------------

    public virtual void Awake() {

    }

    public virtual void Start() {

    }

    public virtual void Init() {

        GameController.Reset();

        foreach(GamePlayerController gamePlayerController in ObjectUtil.FindObjects<GamePlayerController>()) {
            if(gamePlayerController.uuid == UniqueUtil.Instance.currentUniqueId) {
                gamePlayerController.UpdateNetworkContainer(gamePlayerController.uuid);
                break;
            }
        }
        
        GameController.InitGameWorldBounds();
        
        GameController.LoadCharacterTypes();
        GameDraggableEditor.LoadDraggableContainerObject();
        
        Messenger.Broadcast("custom-colors-changed");
    }

    public virtual void OnEnable() {
        Gameverses.GameMessenger<string>.AddListener(
            Gameverses.GameNetworkPlayerMessages.PlayerAdded,
            OnNetworkPlayerContainerAdded);

        Messenger<GameDirectorActor>.AddListener(
            GameDirectorMessages.gameDirectorSpawnActor,
            OnGameDirectorActorLoad);
    }
    
    public virtual void OnDisable() {
        Gameverses.GameMessenger<string>.RemoveListener(
            Gameverses.GameNetworkPlayerMessages.PlayerAdded,
            OnNetworkPlayerContainerAdded);

        Messenger<GameDirectorActor>.RemoveListener(
            GameDirectorMessages.gameDirectorSpawnActor,
            OnGameDirectorActorLoad);
    }


    // ---------------------------------------------------------------------
    
    // PROPERTIES
    
    public int characterActorsCount {
        get {
            return levelActorsContainerObject.transform.childCount;
        }
    }
    
    public int collectableItemsCount {
        get {
            return itemContainerObject.transform.childCount;
        }
    }
    
    // ---------------------------------------------------------------------
    
    // EVENTS
    
    public virtual void OnEditStateHandler(GameDraggableEditEnum state) {
    
        if(state == GameDraggableEditEnum.StateEditing) {
            ////GameHUD.Instance.ShowCurrentCharacter();
        }
        else {

            GameHUD.Instance.ShowGameState();
    
            GameUIController.ShowHUD();
            //ShowUIPanelEditButton();
        }
    }   
    
    // Listen to object creation events and create them such as network players...
    
    public virtual void OnNetworkPlayerContainerAdded(string uuid) {
    
        // Look for object by that uuid, if not create it

        GamePlayerController[] playerControllers = ObjectUtil.FindObjects<GamePlayerController>();
    
        if(playerControllers.Length > 0) {
    
            bool found = false;

            foreach(GamePlayerController gamePlayerController in playerControllers) {
                if(gamePlayerController.uuid == uuid) {
                    // already added
                    gamePlayerController.uuid = uuid;
                    gamePlayerController.UpdateNetworkContainer(uuid);
                    //gamePlayerController.ChangePlayerState(GamePlayerControllerState.ControllerNetwork);
                    LogUtil.Log("Updating character:" + uuid);
                    found = true;
                    break;
                }
            }

            if(!found) {
                // create
                // Prefabs/Characters/GamePlayerObject
    
                UnityEngine.Object prefabGameplayer = Resources.Load("Prefabs/Characters/GamePlayerObject");
                if(prefabGameplayer != null) {
                    Vector3 placementPos = Vector3.zero;
                    placementPos.z = -3f;
                    GamePlayerController playerControllerOther = (Instantiate(prefabGameplayer, placementPos, Quaternion.identity) as GameObject).GetComponent<GamePlayerController>();
                    playerControllerOther.ChangePlayerState(GamePlayerControllerState.ControllerNetwork);
                    playerControllerOther.uuid = uuid;
                    playerControllerOther.UpdateNetworkContainer(uuid);
                    LogUtil.Log("Creating character:" + uuid);
                    LogUtil.Log("playerControllerOther.uuid:" + playerControllerOther.uuid);
                }
            }
        }
    }

    public virtual void OnGameDirectorActorLoad(GameDirectorActor actor) {


    }

    // ---------------------------------------------------------------------

    // GAMEPLAYER CONTROLLER
    
    public virtual GamePlayerController getCurrentPlayerController {
        get {
            return getCurrentController();
        }
    }

    public virtual GamePlayerController getCurrentController() {
        if(GameController.Instance.currentGamePlayerController != null) {
            return GameController.Instance.currentGamePlayerController;
        }
        return null;
    }
 
    // ATTACK
    
    //public static void GamePlayerAttack() {
    //    if(isInst) {
    //        Instance.gamePlayerAttack();
    //    }
    //}
    
    public virtual void gamePlayerAttack() {
        if(GameController.CurrentGamePlayerController != null) {
            GameController.CurrentGamePlayerController.SendAttack();
        }
    }

    //public static void GamePlayerAttackAlt() {
    //    if(isInst) {
    //        Instance.gamePlayerAttackAlt();
    //    }
    //}

    public virtual void gamePlayerAttackAlt() {
        if(GameController.CurrentGamePlayerController != null) {
            GameController.CurrentGamePlayerController.SendAttackAlt();
        }
    }

    //public static void GamePlayerAttackRight() {
    //    if(isInst) {
    //        Instance.gamePlayerAttackRight();
    //    }
    //}

    public virtual void gamePlayerAttackRight() {
        if(GameController.CurrentGamePlayerController != null) {
            GameController.CurrentGamePlayerController.SendAttackRight();
        }
    }

    //public static void GamePlayerAttackLeft() {
    //    if(isInst) {
    //        Instance.gamePlayerAttackLeft();
    //    }
    //}

    public virtual void gamePlayerAttackLeft() {
        if(GameController.CurrentGamePlayerController != null) {
            GameController.CurrentGamePlayerController.SendAttackLeft();
        }
    }

    // DEFEND

    //public static void GamePlayerDefend() {
    //    if(isInst) {
    //        Instance.gamePlayerDefend();
    //    }
    //}

    public virtual void gamePlayerDefend() {
        if(GameController.CurrentGamePlayerController != null) {
            GameController.CurrentGamePlayerController.SendDefend();
        }
    }

    //public static void GamePlayerDefendAlt() {
    //    if(isInst) {
    //        Instance.gamePlayerDefendAlt();
    //    }
    //}

    public virtual void gamePlayerDefendAlt() {
        if(GameController.CurrentGamePlayerController != null) {
            GameController.CurrentGamePlayerController.SendDefendAlt();
        }
    }

    //public static void GamePlayerDefendRight() {
    //    if(isInst) {
    //        Instance.gamePlayerDefendRight();
    //    }
    //}

    public virtual void gamePlayerDefendRight() {
        if(GameController.CurrentGamePlayerController != null) {
            GameController.CurrentGamePlayerController.SendDefendRight();
        }
    }

    //public static void GamePlayerDefendLeft() {
    //    if(isInst) {
    //        Instance.gamePlayerDefendLeft();
    //    }
    //}

    public virtual void gamePlayerDefendLeft() {
        if(GameController.CurrentGamePlayerController != null) {
            GameController.CurrentGamePlayerController.SendDefendLeft();
        }
    }
 
    // JUMP

    //public static void GamePlayerJump() {
    //    if(isInst) {
    //        Instance.gamePlayerJump();
    //    }
    //}
    
    public virtual void gamePlayerJump() {
        if(GameController.CurrentGamePlayerController != null) {
            GameController.CurrentGamePlayerController.InputJump();
        }
    }
     
    // USE
    
    //public static void GamePlayerUse() {
    //    if(isInst) {
    //        Instance.gamePlayerUse();
    //    }
    //}
    
    public virtual void gamePlayerUse() {
        if(GameController.CurrentGamePlayerController != null) {
            GameController.CurrentGamePlayerController.InputUse();
        }
    }

    // SKILL

    //public static void GamePlayerSkill() {
    //    if(isInst) {
    //        Instance.gamePlayerSkill();
    //    }
    //}

    public virtual void gamePlayerSkill() {
        if(GameController.CurrentGamePlayerController != null) {
            GameController.CurrentGamePlayerController.InputSkill();
        }
    }

    // MAGIC

    //public static void GamePlayerMagic() {
    //    if(isInst) {
    //        Instance.gamePlayerMagic();
    //    }
    //}

    public virtual void gamePlayerMagic() {
        if(GameController.CurrentGamePlayerController != null) {
            GameController.CurrentGamePlayerController.InputMagic();
        }
    }

    // ----------------------------------------------------------------------

    // ZONES

    public virtual GameZone getGameZone(GameObject go) {
        if(go != null) {
            return go.GetComponent<GameZone>();
        }
        return null;
    }

    public virtual GameGoalZone getGoalZone(GameObject go) {
        if(go != null) {
            return go.GetComponent<GameGoalZone>();
        }
        return null;
    }

    public virtual GameBadZone getBadZone(GameObject go) {
        if(go != null) {
            return go.GetComponent<GameBadZone>();
        }
        return null;
    }

    public virtual void changeGameZone(GameZones zones) {

        if(gameEndZoneLeft == null) {
            Transform gameEndZoneLeftTransform
                = levelZonesContainerObject.transform.FindChild("GameGoalZoneLeft");
            if(gameEndZoneLeftTransform != null) {
                gameEndZoneLeft = gameEndZoneLeftTransform.gameObject;
            }
        }

        if(gameEndZoneLeft == null) {
            Transform gameEndZoneRightTransform
                = levelZonesContainerObject.transform.FindChild("GameGoalZoneRight");
            if(gameEndZoneRightTransform != null) {
                gameEndZoneRight = gameEndZoneRightTransform.gameObject;
            }
        }

        if(zones != currentGameZone) {
            currentGameZone = zones;

            if(zones == GameZones.left) {

            }
            else if(zones == GameZones.right) {

            }

        }
    }

    // ---------------------------------------------------------------------
    
    // BOUNDS
    
    public virtual void initGameWorldBounds() {
        if(gameBounds == null) {
            gameBounds = gameObject.AddComponent<GameBounds>();
            gameBounds.boundaryTopLeft = boundaryTopLeft;
            gameBounds.boundaryTopRight = boundaryTopRight;
            gameBounds.boundaryBottomLeft = boundaryBottomLeft;
            gameBounds.boundaryBottomRight = boundaryBottomRight;
            gameBounds.boundaryTopCeiling = boundaryTopCeiling;
            gameBounds.boundaryBottomAbyss = boundaryBottomAbyss;
        }
    }

    public virtual bool checkBounds(Vector3 point) {
        return gameBounds.CheckBounds(point);
    }

    public virtual Vector3 filterBounds(Vector3 point) {
        return gameBounds.FilterBounds(point);
    }

    public virtual bool shouldUpdateBounds() {
        return gameBounds.ShouldUpdateBounds();
    }

    // ---------------------------------------------------------------------

    // LEVELS

    public virtual void loadStartLevel(string levelCode) {

        string characterCode = GameProfileCharacters.Current.GetCurrentCharacterCode();
        GameController.LoadCharacterStartLevel(characterCode, levelCode);
    }

    public virtual void loadStartLevel(string characterCode, string levelCode) {
        GameController.LoadCharacterStartLevel(characterCode, levelCode);
    }

    public virtual void startLevel(string levelCode) {
        StartCoroutine(startLevelCo(levelCode));
    }

    public virtual IEnumerator startLevelCo(string levelCode) {

        GameController.ResetCurrentGamePlayer();
        GameController.ResetLevelEnemies();
        
        if(currentGamePlayerController != null) {
            currentGamePlayerController.PlayerEffectWarpFadeIn();
        }
        GameUIPanelOverlays.Instance.ShowOverlayWhite();

        yield return new WaitForSeconds(1f);
        
        GameHUD.Instance.ResetIndicators();
        GameAppController.Instance.LoadLevel(levelCode);
        
        // TODO load anim
        
        yield return new WaitForSeconds(1f);
        
        if(currentGamePlayerController != null) {
            currentGamePlayerController.PlayerEffectWarpFadeOut();
        }
        GameUIPanelOverlays.Instance.HideOverlayWhiteFlashOut();
    }
     
    public virtual void changeLevelFlash() {
        startLevel("1-2");
    }
    
    // ---------------------------------------------------------------------
    // PROFILE
    
    public virtual void loadProfileCharacter(string characterCode) {

        GameProfileCharacters.Current.SetCurrentCharacterCode(characterCode);

        string characterSkinCode = GameProfileCharacters.Current.GetCurrentCharacterCostumeCode();
        GameController.CurrentGamePlayerController.LoadCharacter(characterSkinCode);
    
        GameCustomController.Instance.SetCustomColorsPlayer(
            GameController.CurrentGamePlayerController.gameObject);
    }
    
    public virtual void loadCharacterStartLevel(string characterCode, string levelCode) {
        loadProfileCharacter(characterCode);
        ////GameHUD.Instance.ShowCharacter(characterCode);
        
        GameDraggableEditor.ChangeStateEditing(GameDraggableEditEnum.StateNotEditing);
        startLevel(levelCode);
    }

    public virtual void loadEnemyBot1(float scale, float speed, float attack) {
        GameActorItem character = new GameActorItem();
        character.characterCode = "character-enemy-bot1";
        character.prefabCode = "GameEnemyBot1";
        character.scale = scale;
        character.attack = attack;
        character.speed = speed;
        GameController.LoadActor(character);
    }

    public virtual void loadActor(GameActorItem character) {
        StartCoroutine(loadActorCo(character));
    }

    public virtual Vector3 getCurrentPlayerPosition() {
        Vector3 currentPlayerPosition = Vector3.zero;
        if(GameController.CurrentGamePlayerController != null) {
            if(GameController.CurrentGamePlayerController.gameObject != null) {
                currentPlayerPosition = GameController.CurrentGamePlayerController.gameObject.transform.position;
            }
        }
        return currentPlayerPosition;
    }

    public virtual Vector3 getActorRandomSpawnLocation() {
        Vector3 spawnLocation = Vector3.zero;
        Vector3 currentPlayerPosition = GameController.CurrentPlayerPosition;

        Vector3 boundaryBottomLeftPosition = boundaryEdgeObjectBottomLeft.transform.position;
        Vector3 boundaryBottomRightPosition = boundaryEdgeObjectBottomRight.transform.position;
        Vector3 boundaryTopLeftPosition = boundaryEdgeObjectTopLeft.transform.position;
        Vector3 boundaryTopRightPosition = boundaryEdgeObjectTopRight.transform.position;

        float playerTopLeft = currentPlayerPosition.z - 50f;
        float playerTopRight = currentPlayerPosition.z + 50f;
        float playerBottomLeft = currentPlayerPosition.x - 50f;
        float playerBottomRight = currentPlayerPosition.x + 50f;
    
        //Rect rect = new Rect(0, 0, 150, 150);
        //if (rect.Contains())
        //    print("Inside");
    
        if(playerBottomLeft < boundaryBottomLeftPosition.x) {
            playerBottomLeft = boundaryBottomLeftPosition.x;
        }
        else if(playerBottomRight > boundaryBottomRightPosition.x) {
            playerBottomRight = boundaryBottomRightPosition.x;
        }
        else if(playerTopRight < boundaryTopRightPosition.z) {
            playerTopRight = boundaryTopRightPosition.z;
        }
        else if(playerTopLeft < boundaryTopLeftPosition.z) {
            playerTopLeft = boundaryTopLeftPosition.z;
        }

        spawnLocation.z = UnityEngine.Random.Range(playerTopLeft,playerTopRight);
        spawnLocation.x = UnityEngine.Random.Range(playerBottomLeft,playerBottomRight);
        spawnLocation.y = 0f;

        return spawnLocation;
    }

    public virtual string getCharacterModelPath(GameActorItem character) {
        string modelPath = Contents.appCacheVersionSharedPrefabCharacters;
        // TODO load up dater
        modelPath = PathUtil.Combine(modelPath, "GameEnemyBot1");
        return modelPath;
    }

    public virtual string getCharacterType(GameActorItem character) {
        string type = "bot1";
        // TODO load up
        type = "bot1";
        return type;
    }

    public virtual IEnumerator loadActorCo(GameActorItem character) {

        string modelPath = GameController.GetCharacterModelPath(character);
        string characterType = GameController.GetCharacterType(character);

        // TODO data and pooling and network
    
        UnityEngine.Object prefabObject = Resources.Load(modelPath);
        Vector3 spawnLocation = Vector3.zero;

        bool isZoned = false;

        if(isZoned) {
            // get left/right spawn location
        }
        else {
            // get random
            spawnLocation = GameController.GetActorRandomSpawnLocation();
        }

        if(prefabObject == null) {
            yield break;
        }
    
        GameObject characterObject = Instantiate(
            prefabObject, spawnLocation, Quaternion.identity) as GameObject;
    
        characterObject.transform.parent = levelActorsContainerObject.transform;

        GameCustomController.Instance.SetCustomColorsEnemy(characterObject);
    
        GamePlayerController characterGamePlayerController
            = characterObject.GetComponentInChildren<GamePlayerController>();

        characterGamePlayerController.transform.localScale
            = characterGamePlayerController.transform.localScale * character.scale;

        // Wire up ai controller to setup player health, speed, attack etc.

        //characterGamePlayerController.runtimeData.

        if(characterGamePlayerController != null) {
            characterObject.Hide();
            yield return new WaitForEndOfFrame();
            // wire up properties
    
            // TODO network and player target
            //characterGamePlayerController.currentTarget = GameController.CurrentGamePlayerController.gameObject.transform;
            //characterGamePlayerController.ChangeContextState(GamePlayerContextState.ContextFollowAgent);
            //characterGamePlayerController.ChangePlayerState(GamePlayerControllerState.ControllerAgent);
            characterObject.Show();

            // Add indicator to HUD
    
            GameHUD.Instance.AddIndicator(characterObject, characterType);

            //characterGamePlayerController.Init(GamePlayerControllerState.ControllerAgent);
        }
    }


    // ---------------------------------------------------------------------
    // RESETS
    
    public virtual void resetLevelEnemies() {
        if(levelActorsContainerObject != null) {
            levelActorsContainerObject.DestroyChildren();
        }
    }

    public virtual void resetCurrentGamePlayer() {
        if(currentGamePlayerController != null) {
            currentGamePlayerController.Reset();
        }
    }

    public virtual void reset() {

        GameController.ResetRuntimeData();
        GameController.ResetCurrentGamePlayer();

        GameController.ResetLevelEnemies();
        GameUIController.HideGameCanvas();

        GameDraggableEditor.ClearLevelItems(levelItemsContainerObject);
        GameDraggableEditor.ResetCurrentGrabbedObject();
        GameDraggableEditor.HideAllEditDialogs();
    }

    public virtual void resetRuntimeData() {
        runtimeData = new GameGameRuntimeData();
        runtimeData.ResetTime(defaultLevelTime);
        isGameOver = false;
    }

    // ---------------------------------------------------------------------
    // GAME MODES
    
    public virtual void changeGameMode(GameModeGlobal gameModeTo) {
        if(gameModeTo != gameMode) {
            gameMode = gameModeTo;
        }
    }
    
    public bool isGameModeArcade {
        get {
            if(gameMode == GameModeGlobal.GameModeArcade) {
                return true;
            }
            return false;
        }
    }       
    
    public bool isGameModeChallenge {
        get {
            if(gameMode == GameModeGlobal.GameModeChallenge) {
                return true;
            }
            return false;
        }
    }           
    
    public bool isGameModeTraining {
        get {
            if(gameMode == GameModeGlobal.GameModeTraining) {
                return true;
            }
            return false;
        }
    }

    // ---------------------------------------------------------------------
    // CHARACTER TYPES
    
    public virtual void loadCharacterTypes() {
        foreach(GameCharacterType type in GameCharacterTypes.Instance.GetAll()) {
            if(!gameCharacterTypes.Contains(type.code)) {
                gameCharacterTypes.Add(type.code);
            }
        }
    }
    
    public virtual void cycleCharacterTypesNext() {
        currentCharacterTypeIndex++;
        GameController.CycleCharacterTypes(currentCharacterTypeIndex);
    }
    
    public virtual void cycleCharacterTypesPrevious() {
        currentCharacterTypeIndex--;
        GameController.CycleCharacterTypes(currentCharacterTypeIndex);
    }
    
    public virtual void cycleCharacterTypes(int updatedIndex) {
    
        if(updatedIndex > gameCharacterTypes.Count - 1) {
            currentCharacterTypeIndex = 0;
        }
        else if(updatedIndex < 0) {
            currentCharacterTypeIndex = gameCharacterTypes.Count - 1;
        }
        else {
            currentCharacterTypeIndex = updatedIndex;
        }
    
        if(currentGamePlayerController != null) {
            currentGamePlayerController.LoadCharacter(gameCharacterTypes[currentCharacterTypeIndex]);
        }
    }


    // ---------------------------------------------------------------------
    // GAME MODES
    
    public virtual void restartGame() {
        GameController.Reset();
        GameController.StartGame(GameLevels.Current.code);
    }

    public virtual void startGame(string levelCode) {
        GameController.ChangeGameState(GameStateGlobal.GameStarted);
    }


    public virtual void gameContentDisplay(string contentDisplayCodeTo) {
        contentDisplayCode = contentDisplayCodeTo;
        GameController.ChangeGameState(GameStateGlobal.GameContentDisplay);
    }

    public virtual void pauseGame() {
        GameController.ChangeGameState(GameStateGlobal.GamePause);
    }

    public virtual void resumeGame() {
        GameController.ChangeGameState(GameStateGlobal.GameResume);
    }

    public virtual void quitGame() {
        GameController.ChangeGameState(GameStateGlobal.GameQuit);
    }

    public virtual void resultsGameDelayed() {
        Invoke ("resultsGame", .5f);
    }

    public virtual void resultsGame() {
        GameController.ChangeGameState(GameStateGlobal.GameResults);
    }

    public virtual void togglePauseGame() {
        if(gameState == GameStateGlobal.GamePause) {
            GameController.ResumeGame();
        }
        else {
            GameController.PauseGame();
        }
    }

    // -------------------------------------------------------
    // DIRECTORS
    
    public virtual void runDirectorsDelayed(float delay) {
        StartCoroutine(runDirectorsDelayedCo(delay));
    }

    public virtual IEnumerator runDirectorsDelayedCo(float delay) {
        yield return new WaitForSeconds(delay);
        GameController.RunDirectors();
    }
    
    public virtual void runDirectors() {
        GameController.UpdateDirectors(true);
    }
    
    public virtual void stopDirectors() {
        GameController.UpdateDirectors(false);
    }
    
    public virtual void updateDirectors(bool run) {
        GameAIController.Instance.runDirector = run;
        GameItemController.Instance.runDirector = run;
    }

    // -------------------------------------------------------
    // GAME STATES / HANDLERS   
    
    public virtual void gameRunningStateStopped() {
        Time.timeScale = 1f;
        gameRunningState = GameRunningState.STOPPED;
        GameController.Instance.gameState = GameStateGlobal.GameNotStarted;
    }
    
    public virtual void gameRunningStatePause() {
        Time.timeScale = 0f;
        gameRunningState = GameRunningState.PAUSED;
        GameController.Instance.gameState = GameStateGlobal.GamePause;
    }
    
    public virtual void gameRunningStateRun() {
        Time.timeScale = 1f;
        gameRunningState = GameRunningState.RUNNING;
        GameController.Instance.gameState = GameStateGlobal.GameStarted;
    }

    public virtual void onGameContentDisplay() {
        // Show runtime content display data
        //GameRunningStatePause();
    
        if(contentDisplayCode == GameContentDisplayTypes.gamePlayerOutOfBounds) {

            GameController.GamePlayerOutOfBoundsDelayed(3f);
    
            UIPanelDialogBackground.ShowDefault();
            UIPanelDialogDisplay.SetTitle("OUT OF BOUNDS");
            //UIPanelDialogDisplay.SetDescription("RUN, BUT STAY IN BOUNDS...");
            UIPanelDialogDisplay.ShowDefault();
        }
        else if(contentDisplayCode == GameContentDisplayTypes.gamePlayerOutOfBounds) {

            GameController.GamePlayerOutOfBoundsDelayed(2f);
    
            UIPanelDialogBackground.ShowDefault();
            UIPanelDialogDisplay.SetTitle("OUT OF BOUNDS");
            UIPanelDialogDisplay.SetDescription("RUN, BUT STAY IN BOUNDS...");
            UIPanelDialogDisplay.ShowDefault();
        }
        else {
            UIPanelDialogBackground.HideAll();
        }
    
        //GameRunningStateRun();
    }
    
    public virtual void onGameStarted() {

        GameController.StartLevelStats();
    
        GameController.ResetRuntimeData();
    
        GameUIController.HideUI(true);
        GameUIController.ShowHUD();
    
        if(allowedEditing) {
            GameDraggableEditor.ShowUIPanelEditButton();
        }
    
        GameController.GameRunningStateRun();

        GameUIController.ShowGameCanvas();
    
        GameController.RunDirectorsDelayed(6f);
    }   

    public virtual void onGameQuit() {
    
        // Cleanup
        GameUIController.HideHUD();
        GameDraggableEditor.HideAllEditDialogs();
        GameDraggableEditor.HideAllUIEditPanels();
    
        // Back
        GameUIController.ShowUI();
    
        //ChangeGameState(GameStateGlobal.GameResults);
        // Show dialog then dismiss to not started...
        GameController.Reset();

        GameController.GameRunningStateStopped();
    
        GameController.StopDirectors();
    
        //ChangeGameState(GameStateGlobal.GameNotStarted);
    }
    
    public virtual void onGamePause() {
        // Show pause, resume, quit menu
        GameUIController.ShowUIPanelPause();
        UIPanelDialogBackground.ShowDefault();
        GameController.GameRunningStatePause();
    }
    
    public virtual void onGameResume() {
        GameDraggableEditor.HideAllEditDialogs();
        GameUIController.ShowHUD();
        GameUIController.HideUIPanelPause();
        UIPanelDialogBackground.HideAll();
        GameController.GameRunningStateRun();
    }
    
    public virtual void onGameNotStarted() {
        //
    }
    
    public virtual void onGameResults() {

        LogUtil.Log("OnGameResults");
    
        //if(runtimeData.localPlayerWin){
        //GameUIPanelResults.Instance.ShowSuccess();
        //GameUIPanelResults.Instance.HideFailed();
        //}
        //else {
        //GameUIPanelResults.Instance.HideSuccess();
        //GameUIPanelResults.Instance.ShowFailed();
        //}
    
        GameUIPanelOverlays.Instance.ShowOverlayWhiteStatic();

        GameController.ProcessLevelStats();
        //// Process stats
        //StartCoroutine(processLevelStatsCo());
    
        GameController.StopDirectors();
    }
    
    public virtual void changeGameState(GameStateGlobal gameStateTo) {
        gameState = gameStateTo;
    
        Messenger<GameStateGlobal>.Broadcast(GameMessages.state, gameState);
    
        if(gameState == GameStateGlobal.GameStarted) {
            GameController.OnGameStarted();
        }
        else if(gameState == GameStateGlobal.GamePause) {
            GameController.OnGamePause();
        }
        else if(gameState == GameStateGlobal.GameResume) {
            GameController.OnGameResume();
        }
        else if(gameState == GameStateGlobal.GameQuit) {
            GameController.OnGameQuit();
        }
        else if(gameState == GameStateGlobal.GameNotStarted) {
            GameController.OnGameNotStarted();
        }
        else if(gameState == GameStateGlobal.GameResults) {
            GameController.OnGameResults();
        }
        else if(gameState == GameStateGlobal.GameContentDisplay) {
            GameController.OnGameContentDisplay();
        }
    }

    public bool isGameRunning {
        get {
            if(gameState == GameStateGlobal.GameStarted) {
                return true;
            }
            return false;
        }
    }

    // -------------------------------------------------------
    
    // GAME CAMERA
    
    public virtual void changeGameCameraMode(GameCameraView cameraViewTo) {
        if(cameraViewTo == cameraView) {
            return;
        }
        else {
            cameraView = cameraViewTo;
        }
    
        LogUtil.Log("ChangeGameCameraMode:cameraViewTo: " + cameraViewTo);
    
        if(cameraGame != null
            && cameraGameGround != null) {

            if(cameraView == GameCameraView.ViewSide) {

                Vector3 positionTo = Vector3.zero;
                Vector3 rotationTo = Vector3.zero.WithX(30);

                GameController.ChangeGameCameraProperties(
                    cameraGame.gameObject, positionTo, rotationTo, 2f);

                GameController.ChangeGameCameraProperties(
                    cameraGameGround.gameObject, positionTo, rotationTo, 2f);
            }
            else if(cameraView == GameCameraView.ViewSideTop) {

                Vector3 positionTo = Vector3.zero;
                Vector3 rotationTo = Vector3.zero.WithX(80);

                GameController.ChangeGameCameraProperties(
                    cameraGame.gameObject, positionTo, rotationTo, 2f);

                GameController.ChangeGameCameraProperties(
                    cameraGameGround.gameObject, positionTo, rotationTo, 2f);
            }
            else if(cameraView == GameCameraView.ViewBackTilt) {

                Vector3 positionTo = Vector3.zero;
                Vector3 rotationTo = Vector3.zero.WithX(45).WithY(90);

                GameController.ChangeGameCameraProperties(
                    cameraGame.gameObject, positionTo, rotationTo, 2f);

                GameController.ChangeGameCameraProperties(
                    cameraGameGround.gameObject, positionTo, rotationTo, 2f);
            }
            else if(cameraView == GameCameraView.ViewBackTop) {

                Vector3 positionTo = Vector3.zero;
                Vector3 rotationTo = Vector3.zero.WithX(80).WithY(90);

                GameController.ChangeGameCameraProperties(
                    cameraGame.gameObject, positionTo, rotationTo, 2f);

                GameController.ChangeGameCameraProperties(
                    cameraGameGround.gameObject, positionTo, rotationTo, 2f);
            }
        }
    }
    
    public virtual void changeGameCameraProperties(
        GameObject cameraObject, Vector3 positionTo, Vector3 rotationTo, float timeDelay) {
        //cameraObject.transform.rotation = Quaternion.Euler(rotationTo);
        iTween.RotateTo(cameraObject, rotationTo, timeDelay);
    }

    public virtual void cycleGameCameraMode() {

        LogUtil.Log("CycleGameCameraMode: " + cameraView);

        if(cameraView == GameCameraView.ViewSide) {
            GameController.ChangeGameCameraMode(GameCameraView.ViewSideTop);
        }
        else if(cameraView == GameCameraView.ViewSideTop) {
            GameController.ChangeGameCameraMode(GameCameraView.ViewBackTilt);
        }
        else if(cameraView == GameCameraView.ViewBackTilt) {
            GameController.ChangeGameCameraMode(GameCameraView.ViewBackTop);
        }
        else if(cameraView == GameCameraView.ViewBackTop) {
            GameController.ChangeGameCameraMode(GameCameraView.ViewSide);
        }
    }


    // -------------------------------------------------------

    // GAME PLAYER BOUNDS
    
    public virtual void gamePlayerOutOfBounds() {
        GameAudioController.Instance.PlayWhistle();
        GameAudioController.Instance.PlayOh();
        GameController.GameContentDisplay(GameContentDisplayTypes.gamePlayerOutOfBounds);
    }

    public virtual void gamePlayerOutOfBoundsDelayed(float delay) {
        StartCoroutine(gamePlayerOutOfBoundsDelayedCo(delay));
    }

    public virtual IEnumerator gamePlayerOutOfBoundsDelayedCo(float delay) {

        yield return new WaitForSeconds(delay);

        Debug.Log("GamePlayerOutOfBoundsDelayed:");

        isPlayerOutOfBounds = true;

        Debug.Log("GamePlayerOutOfBoundsDelayed:isPlayerOutOfBounds:" + isPlayerOutOfBounds);

        gameState = GameStateGlobal.GameStarted;

        Debug.Log("GamePlayerOutOfBoundsDelayed:gameState:" + gameState);

        GameController.CheckForGameOver();
    }


    // -------------------------------------------------------

    // STATS HANDLING

    public virtual void processLevelStats() {
        StartCoroutine(processLevelStatsCo());
    }

    public virtual IEnumerator processLevelStatsCo() {
         
        yield return new WaitForSeconds(.5f);
    
        double score = currentGamePlayerController.runtimeData.score;
    
        //int ammo = runtimeData.ammo;
        //double currentLevelTime = runtimeData.currentLevelTime;
        //double ammoScore = ammo * 10;
        double totalScore = score; //(ammoScore + score);// * currentLevelTime * .5f;
    
        //GameUIPanelResults.Instance.SetScore(score.ToString("N0"));
        //GameUIPanelResults.Instance.SetAmmo(ammo.ToString("N0") + " x 10 = " + ammoScore.ToString("N0"));
        //GameUIPanelResults.Instance.SetAmmoScore(ammoScore.ToString("N0"));
        //GameUIPanelResults.Instance.SetLevelCode(runtimeData.levelCode);
        //GameUIPanelResults.Instance.SetLevelDisplayName(runtimeData.levelCode);
        //GameUIPanelResults.Instance.SetTotalScore(totalScore.ToString("N0"));
    
        GamePlayerProgress.Instance.SetStatTotal(GameStatCodes.score, totalScore);
    
        yield return new WaitForEndOfFrame();
    
        GamePlayerProgress.Instance.SetStatHigh(GameStatCodes.score, totalScore);
        
        GameUIPanelResults.Instance.UpdateDisplay(currentGamePlayerController.runtimeData, 0f);
        
        //if(runtimeData.localPlayerWin) {
        //  GamePlayerProgress.Instance.SetStatTotal(GameStatCodes.wins, 1f);
        //}
        //else {
        //  GamePlayerProgress.Instance.SetStatTotal(GameStatCodes.losses, 1f);
        //}
        yield return new WaitForEndOfFrame();
             
        GameController.EndLevelStats();
    
        yield return new WaitForEndOfFrame();
    
        GamePlayerProgress.Instance.ProcessProgressRuntimeAchievements();
        
        yield return new WaitForEndOfFrame();
    
        if(!isAdvancing) {
            GameController.AdvanceToResults();
        }
    
        //GC.Collect();
        //GC.WaitForPendingFinalizers();
        //yield return new WaitForSeconds(8f);
    }
    
    public virtual void advanceToResults() {
        isAdvancing = true;
        Invoke("advanceToResultsDelayed", .5f);
    }
    
    public virtual void advanceToResultsDelayed() {
        GameUIController.ShowUIPanelDialogResults();
        //GameAudio.StartGameLoopForLap(4);
        isAdvancing = false;
        GameController.GameRunningStateStopped();
    }
    
    //public virtual void ProcessStatShot() {
    //    GamePlayerProgress.Instance.SetStatTotal(GameStatCodes.shots, 1f);
    //}

    //public virtual void ProcessStatDestroy() {
    //    GamePlayerProgress.Instance.SetStatTotal(GameStatCodes.destroyed, 1f);
    //}
    
    public virtual void startLevelStats() {
        GamePlayerProgress.Instance.ProcessProgressPack("default");
        GamePlayerProgress.Instance.ProcessProgressAction("level-" + GameLevels.Current.code);
    }
    
    public virtual void endLevelStats() {
        GamePlayerProgress.Instance.EndProcessProgressPack("default");
        GamePlayerProgress.Instance.EndProcessProgressAction("level-" + GameLevels.Current.code);
    }


    // -------------------------------------------------------
    
    // GAME SCORE/CHECK GAME OVER

    public virtual void checkForGameOver() {
    
        //Debug.Log("CheckForGameOver:isGameOver:" + isGameOver);
        //Debug.Log("CheckForGameOver:isGameRunning:" + isGameRunning);

        if(isGameRunning) {
        
            // Check player health/status
            // Go to results if health gone
            
            // if time expired on current round advance to next.
            
            //LogUtil.Log("runtimeData:" + runtimeData.currentLevelTime);
            
            // Check for out of ammo
            // Check if user destroyed or completed mission
            // destroyed all destructable objects
            //if(runtimeData.ammo == 0
            //  || IsLevelDestructableItemsComplete()) {
            
            // TODO HANDLE MODES
            
            if(!isGameOver) {
            
                bool gameOverMode = false;
    
                if(isGameModeArcade) {
                    if(currentGamePlayerController.runtimeData.health <= 0f
                    || runtimeData.timeExpired) {
                        gameOverMode = true;
                    }
                }
                else if(isGameModeChallenge) {
                    if(currentGamePlayerController.runtimeData.health <= 0f
                    || runtimeData.timeExpired) {
                        gameOverMode = true;
                    }
                }
                else if(isGameModeTraining) {
                    if(runtimeData.timeExpired) {
                        gameOverMode = true;
                    }
                }
    
                if(isPlayerOutOfBounds) {
                    Debug.Log("CheckForGameOver:isPlayerOutOfBounds:" + isPlayerOutOfBounds);
                    gameOverMode = true;
                    Debug.Log("CheckForGameOver:gameOverMode:" + gameOverMode);
                }
    
                if(gameOverMode) {
                    isGameOver = true;
                    GameController.ResultsGameDelayed();
                }
    
                runtimeData.SubtractTime(Time.deltaTime);
    
                //if(runtimeData.timeExpired) {
                // Change level/flash
                //ChangeLevelFlash();
                //}
            }
        }
    }

    // -------------------------------------------------------

    // HANDLE TOUCH MOVEMENT - TODO MOVE TO UI CONTROLLER

    public virtual void handleTouchInputPoint(Vector3 point) {
        //&& currentGamePlayerController.thirdPersonController.aimingDirection != Vector3.zero) {

        //bool controlInputTouchFinger = GameProfiles.Current.GetControlInputTouchFinger();
        bool controlInputTouchOnScreen = GameProfiles.Current.GetControlInputTouchOnScreen();

        if(currentGamePlayerController != null) {
            var playerPos = currentGamePlayerController.transform.position;
            var touchPos = Camera.mainCamera.ScreenToWorldPoint(point);
    
            var direction = touchPos - playerPos;
            direction.Normalize();
            var directionNormal = direction.normalized;
    
            //touchPos.Normalize();
            //var touchPosNormalized = touchPos.normalized;
    
            var pointNormalized = point;
            pointNormalized.Normalize();
            pointNormalized = pointNormalized.normalized;

            //Debug.Log("directionNormal:" + directionNormal);
            //Debug.Log("controlInputTouchOnScreen:" + controlInputTouchOnScreen);
    
            bool updateFingerNavigate = true;
    
            if(controlInputTouchOnScreen) {
                // If on screen controls are on don't do touch navigate just off the edge of the
                /// backer on the virtual joystick to prevent random movements.
    
                var center = Vector3.zero;//.WithX(Screen.width/2).WithY(Screen.height/2);
    
                var directionAllow = touchPos - center;
                directionAllow.Normalize();
                var directionAllowNormal = directionAllow.normalized;
    
                //Debug.Log("directionAllowNormal:" + directionAllowNormal);
                //Debug.Log("touchPos:" + touchPos);
                //Debug.Log("pointNormalized:" + pointNormalized);
                //Debug.Log("point:" + point);
    
                if(pointNormalized.y < .2f) {
                    if(pointNormalized.x < .2f) {
                        updateFingerNavigate = false;
                    }
    
                    if(pointNormalized.x > .8f) {
                        updateFingerNavigate = false;
                    }
                }

                //Debug.Log("updateFingerNavigate:" + updateFingerNavigate);
            }
        
            if(updateFingerNavigate) {
                currentGamePlayerController.thirdPersonController.verticalInput = directionNormal.y;
                currentGamePlayerController.thirdPersonController.horizontalInput = directionNormal.x;
                currentGamePlayerController.thirdPersonController.verticalInput2 = 0f;
                currentGamePlayerController.thirdPersonController.horizontalInput2 = 0f;
            }
        }
    }

    // -------------------------------------------------------
    
    // LEVEL ITEMS, DATA, RANDOMIZER

    public virtual List<GameLevelItemAsset> getLevelRandomized() {

        List<GameLevelItemAsset> levelItems = new List<GameLevelItemAsset>();

        return GameController.GetLevelRandomized(levelItems);
    }

    public virtual Vector3 getRandomVectorInGameBounds() {
        return Vector3.zero
            .WithX(UnityEngine.Random.Range(
                gameBounds.boundaryTopLeft.transform.position.x,
                gameBounds.boundaryTopRight.transform.position.x))
            .WithY (UnityEngine.Random.Range(.1f, gameBounds.boundaryTopCeiling.transform.position.y/4));
    }
    
    public virtual List<GameLevelItemAsset> getLevelRandomizedGrid() {

        List<GameLevelItemAsset> levelItems = new List<GameLevelItemAsset>();

        /*
        float gridHeight = 30f;
        float gridWidth = 200f;
        float gridDepth = 5f;
        float gridBoxSize = 5f;

        for(int z = 0; z < gridDepth / gridBoxSize; z++) {

            for(int y = 0; y < gridHeight / gridBoxSize; y++) {

                for(int x = 0; x < gridWidth / gridBoxSize; x++) {
    
                    // Random chance of filling this, also port in level layout types from text

                    int randomChance = UnityEngine.Random.Range(0, 150);

                    if(randomChance > 10 && randomChance < 15) {
                        // Fill level item
                        for(int a = 0; a < UnityEngine.Random.Range(1, 2); a++) {

                            Vector3 gridPos = Vector3.zero
                             .WithX(((gridBoxSize * x) + (a * gridBoxSize)) - gridWidth / 2)
                                 .WithY((gridBoxSize * y) + 1)
                                 .WithZ((gridBoxSize * z));

                            GameLevelItemAsset asset = GameController.GetLevelItemAssetFull(gridPos, "portal", 5,
                                GameLevelItemAssetPhysicsType.physicsStatic, true, false, false, false);

                            levelItems.Add(asset);
                        }
                    }
    
                    if(randomChance <= 8) {

                        // Fill level item

                        for(int a = 0; a < UnityEngine.Random.Range(1, 5); a++) {

                            Vector3 gridPos = Vector3.zero
                             .WithX(((gridBoxSize * x) + gridBoxSize + (a * 5)) - gridWidth / 2)
                             .WithY(((gridBoxSize * y)) + 1)
                             .WithZ(((gridBoxSize * z)));

                            GameLevelItemAsset asset = GameController.GetLevelItemAssetFull(gridPos, "padding", 5,
                                GameLevelItemAssetPhysicsType.physicsStatic, true, false, false, false);

                            levelItems.Add(asset);
                        }
                    }
                }
            }
        }
        */
        return levelItems;
    }
    
    public virtual GameLevelItemAsset getLevelItemAsset(string asset_code,
        string physicsType, bool destructable, bool reactive, bool kinematic, bool gravity) {

        return GameController.GetLevelItemAssetRandom(
            asset_code, 0, physicsType, destructable, reactive, kinematic, gravity);
    }
    
    public virtual GameLevelItemAsset getLevelItemAssetRandom(string asset_code, int countLimit,
        string physicsType, bool destructable, bool reactive, bool kinematic, bool gravity) {

        return GameController.GetLevelItemAssetFull(
            GameController.GetRandomVectorInGameBounds(),
            asset_code, 0, physicsType,
            destructable, reactive, kinematic, gravity);
    }
    
    public virtual GameLevelItemAsset getLevelItemAssetFull(
        Vector3 startPosition,
        string asset_code, 
        int countLimit, 
        string physicsType, 
        bool destructable, 
        bool reactive, 
        bool kinematic, 
        bool gravity) {
        
        GameLevelItemAssetStep step = new GameLevelItemAssetStep();
        step.position.FromVector3(startPosition);
        step.scale.FromVector3(Vector3.one * UnityEngine.Random.Range(.5f, 1.2f));
        step.rotation.FromVector3(Vector3.zero.WithZ(UnityEngine.Random.Range(-.1f, .1f)));

        GameLevelItemAsset asset = new GameLevelItemAsset();
        if(countLimit == 0) {
            asset.asset_code = asset_code;
        }
        else {
            asset.asset_code = asset_code + "-" + UnityEngine.Random.Range(1, countLimit).ToString();
        }

        asset.physics_type = physicsType;
        asset.destructable = destructable;
        asset.reactive = reactive;
        asset.kinematic = kinematic;
        asset.gravity = gravity;
        asset.steps.Add(step);

        return asset;
    }
        
    public virtual List<GameLevelItemAsset> getLevelRandomized(List<GameLevelItemAsset> levelItems) {
        
        for(int i = 0; i < UnityEngine.Random.Range(3, 9); i++) {
            GameLevelItemAsset asset = GameController.GetLevelItemAssetRandom("portal", 5,
            GameLevelItemAssetPhysicsType.physicsStatic, true, false, false, false);
            levelItems.Add(asset);
        }
        
        
        for(int i = 0; i < UnityEngine.Random.Range(5, 20); i++) {          
            GameLevelItemAsset asset = GameController.GetLevelItemAssetRandom("box",3,
            GameLevelItemAssetPhysicsType.physicsStatic, false, true, false, true);
            levelItems.Add(asset);
        }

        /*
        for(int i = 0; i < UnityEngine.Random.Range(0, 3); i++) {
            GameLevelItemAssetStep step = new GameLevelItemAssetStep();
            step.position.FromVector3(GetRandomVectorInGameBounds());
            step.scale.FromVector3(Vector3.one * UnityEngine.Random.Range(.7f, 1.2f));
            step.rotation.FromVector3(Vector3.zero.WithZ(UnityEngine.Random.Range(-.1f, .1f)));
            GameLevelItemAsset asset = new GameLevelItemAsset();
            asset.asset_code = "stone" + UnityEngine.Random.Range(1, 2).ToString();
            asset.physics_type = GameLevelItemAssetPhysicsType.physicsStatic;
            asset.destructable = false;
            asset.reactive = false;
            asset.kinematic = false;
            asset.gravity = false;
            asset.steps.Add(step);
            levelItems.Add(asset);
        }
        
        for(int i = 0; i < UnityEngine.Random.Range(0, 1); i++) {
            GameLevelItemAssetStep step = new GameLevelItemAssetStep();
            step.position.FromVector3(GetRandomVectorInGameBounds());
            step.scale.FromVector3(Vector3.one * UnityEngine.Random.Range(.3f, .7f));
            step.rotation.FromVector3(Vector3.zero.WithZ(UnityEngine.Random.Range(-.1f, .1f)));
            GameLevelItemAsset asset = new GameLevelItemAsset();
            asset.asset_code = "stone-spinny-thing";
            asset.physics_type = GameLevelItemAssetPhysicsType.physicsStatic;
            asset.destructable = false;
            asset.reactive = false;
            asset.kinematic = false;
            asset.gravity = false;
            asset.rotation_speed.FromVector3(Vector3.zero.WithZ(UnityEngine.Random.Range(-10f, 20f)));
            asset.steps.Add(step);
            levelItems.Add(asset);
        }
        
        for(int i = 0; i < UnityEngine.Random.Range(0, 1); i++) {
            GameLevelItemAssetStep step = new GameLevelItemAssetStep();
            step.position.FromVector3(GetRandomVectorInGameBounds());
            step.scale.FromVector3(Vector3.one * UnityEngine.Random.Range(.5f, 1f));
            step.rotation.FromVector3(Vector3.zero.WithZ(UnityEngine.Random.Range(-.1f, .1f)));
            GameLevelItemAsset asset = new GameLevelItemAsset();
            asset.asset_code = "stone-spinny-thing2";
            asset.physics_type = GameLevelItemAssetPhysicsType.physicsStatic;
            asset.destructable = false;
            asset.reactive = false;
            asset.kinematic = false;
            asset.gravity = false;
            asset.rotation_speed.FromVector3(Vector3.zero.WithZ(UnityEngine.Random.Range(-10f, 50f)));
            asset.steps.Add(step);
            levelItems.Add(asset);
        }
        
        for(int i = 0; i < UnityEngine.Random.Range(0, 1); i++) {
            GameLevelItemAssetStep step = new GameLevelItemAssetStep();
            step.position.FromVector3(GetRandomVectorInGameBounds());
            step.scale.FromVector3(Vector3.one * UnityEngine.Random.Range(.5f, 1f));
            step.rotation.FromVector3(Vector3.zero.WithZ(UnityEngine.Random.Range(-.1f, .1f)));
            GameLevelItemAsset asset = new GameLevelItemAsset();
            asset.asset_code = "blocks-gray-large";
            asset.physics_type = GameLevelItemAssetPhysicsType.physicsStatic;
            asset.destructable = false;
            asset.reactive = false;
            asset.kinematic = false;
            asset.gravity = false;
            asset.rotation_speed.FromVector3(Vector3.zero.WithZ(UnityEngine.Random.Range(-10f, 50f)));
            asset.steps.Add(step);
            levelItems.Add(asset);
        }
        
        for(int i = 0; i < UnityEngine.Random.Range(0, 1); i++) {
            GameLevelItemAssetStep step = new GameLevelItemAssetStep();
            step.position.FromVector3(GetRandomVectorInGameBounds());
            step.scale.FromVector3(Vector3.one * UnityEngine.Random.Range(.5f, 1f));
            step.rotation.FromVector3(Vector3.zero.WithZ(UnityEngine.Random.Range(-.1f, .1f)));
            GameLevelItemAsset asset = new GameLevelItemAsset();
            asset.asset_code = "blocks-gray-small";
            asset.physics_type = GameLevelItemAssetPhysicsType.physicsStatic;
            asset.destructable = false;
            asset.reactive = false;
            asset.kinematic = false;
            asset.gravity = false;
            asset.rotation_speed.FromVector3(Vector3.zero);
            asset.steps.Add(step);
            levelItems.Add(asset);
        }
        
        for(int i = 0; i < UnityEngine.Random.Range(0, 1); i++) {
            GameLevelItemAssetStep step = new GameLevelItemAssetStep();
            step.position.FromVector3(GetRandomVectorInGameBounds());
            step.scale.FromVector3(Vector3.one * UnityEngine.Random.Range(.5f, 1f));
            step.rotation.FromVector3(Vector3.zero.WithZ(UnityEngine.Random.Range(-.1f, .1f)));
            GameLevelItemAsset asset = new GameLevelItemAsset();
            asset.asset_code = "rocket";
            asset.physics_type = GameLevelItemAssetPhysicsType.physicsStatic;
            asset.destructable = true;
            asset.reactive = false;
            asset.kinematic = false;
            asset.gravity = false;
            asset.rotation_speed.FromVector3(Vector3.zero);
            asset.steps.Add(step);
            levelItems.Add(asset);
        }
        */

        return levelItems;
    }

    // -------------------------------------------------------
    
    // UPDATE
    
    // Update is called once per frame
    public virtual void Update () {
    
        //bool controlInputTouchFinger = GameProfiles.Current.GetControlInputTouchFinger();
        //bool controlInputTouchOnScreen = GameProfiles.Current.GetControlInputTouchOnScreen();
        
        if(isGameRunning) {
            GameController.CheckForGameOver();
        }
        
        //if(controlInputTouchFinger) {

        if(Input.touchCount > 0) {
            foreach(Touch touch in Input.touches) {
                GameController.HandleTouchInputPoint(touch.position);
            }
        }
        else if(Input.GetMouseButtonDown(0)) {
            GameController.HandleTouchInputPoint(Input.mousePosition);
        }
        else {
            if(currentGamePlayerController != null) {
                currentGamePlayerController.thirdPersonController.verticalInput = 0f;
                currentGamePlayerController.thirdPersonController.horizontalInput = 0f;
                currentGamePlayerController.thirdPersonController.verticalInput2 = 0f;
                currentGamePlayerController.thirdPersonController.horizontalInput2 = 0f;
            }
        }
        //}
        
        if(gameState == GameStateGlobal.GamePause
        || GameDraggableEditor.appEditState == GameDraggableEditEnum.StateEditing) {
            return;
        }
        
        currentTimeBlock += Time.deltaTime;     
        
        if(currentTimeBlock > actionInterval) {
            currentTimeBlock = 0.0f;
        }       
        
        // debug/dev
        
        if(Application.isEditor) {
            if(Input.GetKeyDown(KeyCode.L)) {
                GameController.LoadEnemyBot1(UnityEngine.Random.Range(1.5f, 2.5f),
                UnityEngine.Random.Range(.3f, 1.3f),
                UnityEngine.Random.Range(.3f, 1.3f));
            }
            else if(Input.GetKeyDown(KeyCode.K)) {
                GameController.LoadEnemyBot1(UnityEngine.Random.Range(1.5f, 2.5f),
                UnityEngine.Random.Range(.3f, 1.3f),
                UnityEngine.Random.Range(.3f, 1.3f));
            }
            else if(Input.GetKeyDown(KeyCode.J)) {
                GameController.LoadEnemyBot1(UnityEngine.Random.Range(1.5f, 2.5f),
                UnityEngine.Random.Range(.3f, 1.3f),
                UnityEngine.Random.Range(.3f, 1.3f));
            }
        }
    }
    

    // ----------------------------------------------------------------------

    // EXTRA

    public virtual Vector3 cardinalAngles(Vector3 pos1, Vector3 pos2) {
    
        // Adjust both positions to be relative to our origin point (pos1)
        pos2 -= pos1;
        pos1 -= pos1;
    
        Vector3 angles = Vector3.zero;
    
        // Rotation to get from World +Z to pos2, rotated around World X (degrees up from Z axis)
        angles.x = Vector3.Angle( Vector3.forward, pos2 - Vector3.right * pos2.x );
    
        // Rotation to get from World +Z to pos2, rotated around World Y (degrees right? from Z axis)
        angles.y = Vector3.Angle( Vector3.forward, pos2 - Vector3.up * pos2.y );
    
        // Rotation to get from World +X to pos2, rotated around World Z (degrees up from X axis)
        angles.z = Vector3.Angle( Vector3.right, pos2 - Vector3.forward * pos2.z );
    
        return angles;
    }
    
    public virtual float contAngle(Vector3 fwd, Vector3 targetDir, Vector3 upDir) {
        var angle = Vector3.Angle(fwd, targetDir);
    
        if (angleDir(fwd, targetDir, upDir) == -1) {
            return 360 - angle;
        }
        else {
            return angle;
        }
    }
    
    //returns -1 when to the left, 1 to the right, and 0 for forward/backward
    public virtual float angleDir(Vector3 fwd, Vector3 targetDir, Vector3 up) {
    
        Vector3 perp = Vector3.Cross(fwd, targetDir);
    
        float dir = Vector3.Dot(perp, up);
    
        if (dir > 0.0) {
            return 1.0f;
        }
        else if (dir < 0.0) {
            return -1.0f;
        }
        else {
            return 0.0f;
        }
    }
}