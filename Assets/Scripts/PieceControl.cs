using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PieceControl : MonoBehaviour
{
    public int ARR; //Auto Repeat Rate - the speed pieces move while holding a movement key
    public int DAS; //Delayed Auto Shift - the delay before the piece auto repeats movement
    public int SDF = 20; //Soft Drop Factor - amount the soft drop input multiplies drop rate
    public int HDD; //Hard Drop Delay - amount of grace period after a hard drop where the input is disabled
    [SerializeField] private int Buffer = 8; //Input buffer - amount of time an input will be held if it isn't possible as soon as it's pressed
    [SerializeField] private Sprite MinoSprite;
    [SerializeField] private Sprite GhostSprite;
    [SerializeField] private LayerMask CollisionMask;
    [SerializeField] private GameObject PauseMenu;
    [SerializeField] private Button ResumeButton;
    [SerializeField] private GameObject FailMenu;
    [SerializeField] private Button RestartButton;
    [SerializeField] private GameObject scoreDisplay;
    [SerializeField] private GameObject levelDisplay;
    [SerializeField] private GameObject lineDisplay;
    [SerializeField] private GameObject startText;
    [SerializeField] private AudioSource TargetEmitter;
    [SerializeField] private AudioSource MusicEmitter;
    [SerializeField] private AudioClip StartAudio;
    [SerializeField] private AudioClip Start1Audio;
    [SerializeField] private AudioClip Start2Audio;
    [SerializeField] private AudioClip FailAudio;
    [SerializeField] private AudioClip MoveAudio;
    [SerializeField] private AudioClip RotateAudio;
    [SerializeField] private AudioClip HardDropAudio;
    [SerializeField] private AudioClip SoftDropAudio;
    [SerializeField] private AudioClip LandAudio;
    [SerializeField] private AudioClip LockAudio;
    [SerializeField] private AudioClip HoldAudio;
    [SerializeField] private AudioClip TSpinAudio;

    public int GameSpeed = 60; //Overall speed of the game (lower = faster)
    public int Score = 0;
    public int totalLines = 0;
    private int requiredLines = 10;
    public int Level = 1;
    public int Combo = 0;
    public int B2B = 0;
    [Range (0,6)] public int CurrentPiece = 0;
    [Range (0, 3)] public int Rotation = 0;

    [Range (0,6)] private int NextPiece = 0;
    [Range (0,7)] private int HoldPiece = 7;

    private string saveFile;
    private SaveSettings toLoad = new SaveSettings();
    private float MusicVolume;
    private List<int> UsedPieces = new List<int>();
    private float currentMove = 0f;
    private int arTime = 0;
    private int fallTime = 0;
    private int dropDelay = 0;
    private int graceAmount = 80;
    private int moveBuffer = 0;
    private float moveBufferDir = 0;
    private int rotateBuffer = 0;
    private float rotateBufferDir = 0;
    private int grace = 0;
    private bool isAr = false;
    private bool isGrace = false;
    private bool isHD = false;
    private bool paused = false;
    private bool failed = false;
    private bool stop = false;
    private bool hasHolded = false;
    private bool lastRotated = false;
    private bool starting = false;
    private int startTick = 0;
    private int startStage = 0;
    
    private static Color[] pieceColors =
    {
        new Color32(90,252,255,255),
        new Color32(31,44,229,255),
        new Color32(255,106,9,255),
        new Color32(246,246,7,255),
        new Color32(44,236,28,255),
        new Color32(224,15,243,255),
        new Color32(241,31,37,255)
    };

    private static Vector2[,] pieceLayouts =
    {
        { //I Piece 0
            new Vector2(-1.5f, 0.5f),
            new Vector2(-0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(1.5f, 0.5f),
        },
        { //J Piece 1
            new Vector2(-1, 1),
            new Vector2(-1, 0),
            new Vector2(0, 0),
            new Vector2(1, 0),
        },
        { //L Piece 2
            new Vector2(-1, 0),
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(1, 1),
        },
        { //O Piece 3
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(1, 1),
            new Vector2(0, 1),
        },
        { //S Piece 4
            new Vector2(-1, 0),
            new Vector2(0, 0),
            new Vector2(0, 1),
            new Vector2(1, 1),
        },
        { //T Piece 5
            new Vector2(-1, 0),
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
        },
        { //Z Piece 6
            new Vector2(-1, 1),
            new Vector2(0, 1),
            new Vector2(0, 0),
            new Vector2(1, 0),
        }
    };

    private static Vector2[,] wallKickNorm =
    {
        { //0->1, 2->1 tests
            new Vector2(0, 0),
            new Vector2(-1, 0),
            new Vector2(-1, 1),
            new Vector2(0, -2),
            new Vector2(-1, -2)
        },
        { //1->0, 1->2 tests
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(1, -1),
            new Vector2(0, 2),
            new Vector2(1, 2)
        },
        { //2->3, 0->3 tests
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(1, 1),
            new Vector2(0, -2),
            new Vector2(1, -2)
        },
        { //3->2, 3->0 tests
            new Vector2(0, 0),
            new Vector2(-1, 0),
            new Vector2(-1, -1),
            new Vector2(0, 2),
            new Vector2(-1, 2)
        }
    };
    private static Vector2[,] wallKickI =
    {
        { //0->1 or 3->2 tests, inverse for 2->3 or 1->0
            new Vector2(0f, 0f),
            new Vector2(-2f, 0f),
            new Vector2(1f, 0f),
            new Vector2(-2f, -1f),
            new Vector2(1f, 2f)
        },
        { //1->2 or 0->3 tests, inverse for 3->0 or 2->1
            new Vector2(0f, 0f),
            new Vector2(-1f, 0f),
            new Vector2(2f, 0f),
            new Vector2(-1f, 2f),
            new Vector2(2f, -1f)
        }
    };

    Vector2[] tSpinChecks =
    {
            new Vector2(-1, 1),
            new Vector2(1, 1),
            new Vector2(-1, -1),
            new Vector2(1, -1)
    };

    void LoadOptions()
    {
        if(File.Exists(saveFile)) {
            string saveContents = File.ReadAllText(saveFile);

            toLoad = JsonUtility.FromJson<SaveSettings>(saveContents);

            DAS = toLoad.DelayAutoShift;
            ARR = toLoad.AutoRepeatRate;
            SDF = toLoad.SoftDropFactor;
            MusicVolume = toLoad.MusicVolume;
            if(toLoad.HardDropDelay) {
                HDD = 8;
            } else {HDD = 0;}
        } else {
            DAS = 9;
            ARR = 3;
            HDD = 8;
            MusicVolume = 0.5f;
        }
    }

    void CheckNextPiece() //Change the next piece to something not already used in the bag
    {
        if(UsedPieces.Count > 6) { //Refresh the bag if it's all used
            UsedPieces.Clear();
        }
        List<int> unused = new List<int>();
        for(int n = 0; n < 7; n++) //Get a list of all unused pieces
        {
            bool exists = UsedPieces.Contains(n);
            if(!exists) {
                unused.Add(n);
            }
        }
        NextPiece = unused[Random.Range(0, unused.Count)]; //Pick a random piece from the list of unused pieces
    }
    void GetNextPiece() //Set the current piece to the old "next" piece, and randomize the next "next" piece
    {
        UsedPieces.Add(NextPiece);
        CurrentPiece = NextPiece;
        CheckNextPiece();
    }

    Vector2 InverseVector2(Vector2 inputVector) //Outputs the inverse of any input Vector2
    {
        Vector2 outputVector = Vector2.zero;
        outputVector.x = inputVector.x*-1;
        outputVector.y = inputVector.y*-1;
        return(outputVector);
    }

    bool CheckMino(Vector2 checkPos) //Returns false if the mino is invalid, true if valid
    {
        if(checkPos.x < -4.5 || checkPos.x > 4.5 || checkPos.y < -9.5) { //Check if the mino is out of bounds
            return(false);
        } else {
            bool isCollided = Physics2D.OverlapBox(checkPos, new Vector2(0.1f,0.1f), 0, CollisionMask); //Check if the mino collides with other minos
            if(isCollided) {
                return(false);
            } else return(true);
        }
    }

    bool CheckPiece(Vector2 checkPos, int checkRotation) //Returns false if the piece is invalid, true if valid. Nearly identical to "DrawPiece"
    {
        for(int n = 0; n < 4; n++)
        {
            Vector2 baseOffset = pieceLayouts[CurrentPiece, n];
            Vector2 offset = new Vector2(0,0);
            if(CurrentPiece == 3) {
                offset = baseOffset;
            } else if(checkRotation == 1) {
                offset.x = baseOffset.y;
                offset.y = baseOffset.x*-1;
            } else if(checkRotation == 2) {
                offset.x = baseOffset.x*-1;
                offset.y = baseOffset.y*-1;
            } else if(checkRotation == 3) {
                offset.x = baseOffset.y*-1;
                offset.y = baseOffset.x;
            } else {
                offset = baseOffset;
            }

            Vector2 drawPos = checkPos + offset;
            bool isValid = CheckMino(drawPos);
            if(!isValid) {
                return(false);
            }
        }
        return(true);
    }

    int CheckSpin(Vector2 checkPos, int checkRotation) //Returns a value from 0-2, 0 = no spin, 1 = mini T-spin, 2 = T-Spin (note: only checks position)
    {
        int frontCount = 0;
        int backCount = 0;

        for(int n = 0; n < 4; n++)
        {
            Vector2 baseOffset = tSpinChecks[n];
            Vector2 offset = new Vector2(0,0);
            if(checkRotation == 1) {
                offset.x = baseOffset.y;
                offset.y = baseOffset.x*-1;
            } else if(checkRotation == 2) {
                offset.x = baseOffset.x*-1;
                offset.y = baseOffset.y*-1;
            } else if(checkRotation == 3) {
                offset.x = baseOffset.y*-1;
                offset.y = baseOffset.x;
            } else {
                offset = baseOffset;
            }

            Vector2 drawPos = checkPos + offset;
            bool isValid = CheckMino(drawPos);
            if(!isValid) {
                if(baseOffset.y > 0) {
                    frontCount++;
                } else {
                    backCount++;
                }
            }
        }

        if(frontCount == 2 && backCount > 0) {
            return(2);
        } else if(backCount == 2 && frontCount > 0) {
            return(1);
        } else {return(0);}
    }

    void DrawMino(string objectName, Sprite drawSprite, Vector2 drawPos, Color drawColor, bool isLocked, int order)
    {
        GameObject Mino = new GameObject(objectName); //Create a new game object
        if(!isLocked) {
            Mino.transform.SetParent(transform); //Child the game object to the controller if not locked
        }
        Mino.transform.position = drawPos;

        SpriteRenderer renderer = Mino.AddComponent<SpriteRenderer>(); //Give the game object a sprite
        renderer.sprite = drawSprite;
        renderer.color = drawColor;
        renderer.sortingOrder = order;
        if(isLocked) { //Set the proper attributes if locked
            GameObject targetParent = GameObject.Find("GameBoard");
            Mino.transform.SetParent(targetParent.transform);
            Collider2D minoCollider = Mino.AddComponent<BoxCollider2D>();
            Mino.layer = 3;
            Mino.tag = "PlacedMino";
        }
    }

    void DrawPiece(int pieceShape, int drawRotation, string minoName, Sprite drawSprite, Vector2 drawPos, Color drawColor, bool isLocked, int order)
    {
        for(int n = 0; n < 4; n++) //For each of the 4 minos, consult the arrays for where to draw
        {
            Vector2 baseOffset = pieceLayouts[pieceShape, n];
            Vector2 offset = new Vector2(0,0);
            if(pieceShape == 3) { //Don't bother rotating on squares, nerd
                offset = baseOffset;
            } else if(drawRotation == 1) {
                offset.x = baseOffset.y;
                offset.y = baseOffset.x*-1;
            } else if(drawRotation == 2) {
                offset.x = baseOffset.x*-1;
                offset.y = baseOffset.y*-1;
            } else if(drawRotation == 3) {
                offset.x = baseOffset.y*-1;
                offset.y = baseOffset.x;
            } else {
                offset = baseOffset;
            }

            Vector2 minoPos = drawPos + offset;
            DrawMino(minoName + n, drawSprite, minoPos, drawColor, isLocked, order);
        }
    }

    void GameOver()
    {
        stop = true;
        failed = true;
        paused = false;

        PauseMenu.SetActive(false);
        FailMenu.SetActive(true);

        MusicEmitter.Stop();
        TargetEmitter.PlayOneShot(FailAudio);
    }

    void StartGame()
    {
        MusicEmitter.volume = MusicVolume;
        MusicEmitter.Play();

        stop = false;
        FailMenu.SetActive(false);
    }

    void Restart()
    {
        GameObject board = GameObject.Find("GameBoard");
        ClearLines clearScript = board.GetComponent<ClearLines>();
        Text levelText = levelDisplay.GetComponent<Text>();
        Text lineText = lineDisplay.GetComponent<Text>();
        Text scoreText = scoreDisplay.GetComponent<Text>();

        clearScript.ClearBoard();

        Score = 0;
        totalLines = 0;
        requiredLines = 10;
        Level = 1;
        GameSpeed = 60;
        graceAmount = 80;
        UsedPieces.Clear();

        Vector2 newSpawn = Vector2.zero;
        CurrentPiece = Random.Range(0, 7);
        UsedPieces.Add(CurrentPiece);
        Rotation = 0;

        if(CurrentPiece == 0) {
            newSpawn = new Vector2(0,12);
        } else {
            newSpawn = new Vector2(-0.5f, 11.5f);
        }
        transform.position = newSpawn;

        CheckNextPiece();
        HoldPiece = 7;
        grace = graceAmount;

        string displayLines = totalLines.ToString();
        string displayTarget = requiredLines.ToString();
        
        lineText.text = displayLines + "/" + displayTarget;
        levelText.text = Level.ToString();
        scoreText.text = "0000";

        failed = false;
        FailMenu.SetActive(false);

        starting = true;
        startStage = 0;
        startTick = 60;
    }

    void UnPause()
    {
        if(!failed) {
            stop = false;
            paused = false;

            MusicEmitter.UnPause();
            PauseMenu.SetActive(false);
        }
    }

    void LockPiece()
    {
        GameObject board = GameObject.Find("GameBoard");
        ClearLines clearScript = board.GetComponent<ClearLines>();
        Vector2 newSpawn;
        Text levelText = levelDisplay.GetComponent<Text>();
        Text lineText = lineDisplay.GetComponent<Text>();
        Text scoreText = scoreDisplay.GetComponent<Text>();
        int isSpin = 0;

        DrawPiece(CurrentPiece, Rotation, "Block", MinoSprite, transform.position, pieceColors[CurrentPiece], true, 0);

        if(CurrentPiece == 5 && lastRotated) {
            isSpin = CheckSpin(transform.position, Rotation);
        }

        int gainedScore = clearScript.LineClear(CurrentPiece, Rotation, transform.position, isSpin); //Run the line clear script and get a score value for our clears
        lastRotated = false;

        if(totalLines >= requiredLines) { // Level up if enough lines are cleared
            Level++;
            requiredLines += 5*Level;
            GameSpeed = 60 / Level;
            if(graceAmount > 10) {
                graceAmount -= Level/2;
            }
            if(graceAmount < 10) {
                graceAmount = 10;
            }
        }
        string displayLines = totalLines.ToString();
        string displayTarget = requiredLines.ToString();
        
        lineText.text = displayLines + "/" + displayTarget;
        levelText.text = Level.ToString();

        string renderScore = "0000";
        if(Score < 10) {
            renderScore = "000" + Score;
        } else if(Score < 100) {
            renderScore = "00" + Score;
        } else if(Score < 1000) {
            renderScore = "0" + Score;
        } else if(Score < 10000) {
            renderScore = Score.ToString();
        } else {renderScore = "FUCK";}
        scoreText.text = renderScore;

        if(NextPiece == 0) { //Move the play piece back to the top of the board
            newSpawn = new Vector2(0,12);
        } else {
            newSpawn = new Vector2(-0.5f, 11.5f);
        }
        bool canSpawn = CheckPiece(newSpawn, 0);
        if(!canSpawn) {
            GameOver();
            return;
        }
        transform.position = newSpawn;
        Rotation = 0;

        GetNextPiece(); //Get the next piece
        moveBuffer = 0; //Reset misc. variables
        rotateBuffer = 0;
        fallTime = 0;
        isGrace = false;
        hasHolded = false;
        dropDelay = HDD;
        grace = graceAmount;
        TargetEmitter.PlayOneShot(LockAudio);
    }

    Vector2 CheckFall() //Returns the farthest down position the piece can currently fall
    {
        Vector2 drawPos = new Vector2(0,0);
        Vector2 checkPos = transform.position;
        bool valid = true;

        while(valid) //Move the piece down until it hits an invalid state
        {
            checkPos.y -= 1;

            valid = CheckPiece(checkPos, Rotation);
            if(!valid) { //Upon hitting an invalid option, move up one and leave the loop
                checkPos.y += 1;
                drawPos = checkPos;
            }
        }

        return(drawPos);
    }

    bool ApplyMove(Vector2 moveAmount) //Attempts to apply a vector to piece origin, returns true if successful
    {
        Vector2 originPos = transform.position;
        originPos += moveAmount;
        bool validMovement = CheckPiece(originPos, Rotation);

        if(validMovement) {
            transform.position = originPos;
            lastRotated = false;
            return(true);
        } else return(false);
    }

    Vector2 KickTest(Vector2 initialPos, int testRotation, float rotateDir) //Test where a piece should move when rotating, returning a Vector if successful
    {
        int testSet = 0;

        if(CurrentPiece != 0) {
            if(rotateDir > 0) {
                if(testRotation == 0) {
                    testSet = 3;
                } else testSet = testRotation - 1;
            }
            else if(testRotation == 1) {
                testSet = 0;
            } else if(testRotation == 0) {
                testSet = 1;
            } else if(testRotation == 3) {
                testSet = 2;
            } else if(testRotation == 2) {
                testSet = 3;
            }

            for(int n = 0; n < 5; n++)
            {
                Vector2 testPos = initialPos + wallKickNorm[testSet, n];
                bool isValid = CheckPiece(testPos, testRotation);
                if(isValid) {
                    return(testPos);
                }
            }
        }
        else if(CurrentPiece == 0) { //Use a different set of tests for I pieces than others
            bool inverse = false;
            if(rotateDir > 0) { //Please don't look at this terrible logic
                if(testRotation == 3) {
                    inverse = true;
                }
                else if(testRotation == 2 || testRotation == 0) {
                    testSet = 1;
                    if(testRotation == 0) {
                        inverse = true;
                    }
                }
            } else if(testRotation == 0) {
                inverse = true;
            }
            else if(testRotation == 1 || testRotation == 3) {
                testSet = 1;
                if(testRotation == 1) {
                    inverse = true;
                }
            }

            for(int n = 0; n < 5; n++)
            {
                Vector2 testPos = wallKickI[testSet, n];
                if(inverse) {testPos = InverseVector2(testPos);} //The vector is inversed if needed
                testPos += initialPos;
                bool isValid = CheckPiece(testPos, testRotation);
                if(isValid) {
                    return(testPos);
                }
            }
        }
        return(new Vector2(float.MinValue, float.MinValue));
    }

    bool ApplyGravity()
    {
        bool didFall = ApplyMove(new Vector2(0,-1));

        Vector2 toCheck = transform.position;
        toCheck.y--;
        bool canFall = CheckPiece(toCheck, Rotation);

        if(!didFall) { //Activate grace if the piece can't fall
            isGrace = true;
        }
        if(!canFall && !isGrace) {
            TargetEmitter.PlayOneShot(LandAudio);
        }
        fallTime = 0;
        return(didFall);
    }

    public void Pause(InputAction.CallbackContext context)
    {
        if(context.started && !failed) {
            paused = !paused;
            if(paused) {
                stop = true;
                PauseMenu.SetActive(true);
                MusicEmitter.Pause();
            } else {
                UnPause();
            }
        }
    }

    public void Move(InputAction.CallbackContext context) //Returns true if succsessful, returns false if not
    {
        float moveAmount = context.ReadValue<float>();

        if(context.started && !stop) {
            Vector2 moveApply = new Vector2(moveAmount, 0);
            bool isValid = ApplyMove(moveApply);
            if(isValid) {
                moveBuffer = 0;
                TargetEmitter.PlayOneShot(MoveAudio);
            } else if(moveBuffer == 0) {
                moveBufferDir = moveAmount;
                moveBuffer = Buffer;
            }
        }
        if(context.performed) {
            currentMove = moveAmount;
        }
        if(context.canceled) {
            currentMove = 0;
        }
    }

    public void Rotate(InputAction.CallbackContext context) //Returns true if succsessful, returns false if not
    {
        int testRotation = Rotation;
        float rotateDir = context.ReadValue<float>();
        Vector2 currentPos = transform.position;

        if(context.started && !stop) {
            if(CurrentPiece == 3) {
                rotateBuffer = 0;
                return;
            }
            if(rotateDir > 0) {
                testRotation++;
                if(testRotation > 3) {
                    testRotation = 0;
                }
            }
            if(rotateDir < 0) {
                testRotation--;
                if(testRotation < 0) {
                    testRotation = 3;
                }
            }
            Vector2 newPos = KickTest(transform.position, testRotation, rotateDir);
            bool isValid = newPos.y > -1000; //This is a scuffed but also kinda funny way to check if the rotation is valid

            if(isValid) {
                int doesSpin = CheckSpin(newPos, testRotation);
                rotateBuffer = 0;
                Rotation = testRotation;
                ApplyMove(newPos - currentPos);
                lastRotated = true;
                if(doesSpin > 0 && CurrentPiece == 5) { //Play a T-Spin audio if needed
                    TargetEmitter.PlayOneShot(TSpinAudio);
                }
                TargetEmitter.PlayOneShot(RotateAudio);
            } else if(rotateBuffer == 0){
                rotateBuffer = Buffer;
                rotateBufferDir = rotateDir;
            }
        }
    }

    public void SoftDrop(InputAction.CallbackContext context)
    {
        if(context.started && !stop) {
            bool didFall = ApplyGravity();
            if(didFall) {
                TargetEmitter.PlayOneShot(SoftDropAudio);
            }
        }
        if(context.performed) {
            isHD = true;
        }
        if(context.canceled) {
            isHD = false;
        }
    }

    public void HardDrop(InputAction.CallbackContext context)
    {
        Vector2 drawPos = CheckFall();
        Vector2 currentPos = transform.position;

        if(context.started && dropDelay == 0 && !stop) {
            if(currentPos != drawPos) {
                lastRotated = false;
                TargetEmitter.PlayOneShot(HardDropAudio);
            }

            transform.position = drawPos;
            LockPiece();
        }
    }

    public void Hold(InputAction.CallbackContext context)
    {
        if(context.started && !stop && !hasHolded) {
            Vector2 newSpawn;

            if(HoldPiece == 7) {
                HoldPiece = CurrentPiece;
                GetNextPiece();
            } else {
                int toHold = CurrentPiece;
                CurrentPiece = HoldPiece;
                HoldPiece = toHold;
            }

            if(CurrentPiece == 0) {
                newSpawn = new Vector2(0,12);
            } else {
                newSpawn = new Vector2(-0.5f, 11.5f);
            }
            transform.position = newSpawn;
            Rotation = 0;
            isGrace = false;
            grace = graceAmount;

            lastRotated = false;
            hasHolded = true;
            TargetEmitter.PlayOneShot(HoldAudio);
        }
    }

    void Start()
    {
        saveFile = Application.persistentDataPath + "/settings.json";

        ResumeButton.onClick.AddListener(UnPause);
        RestartButton.onClick.AddListener(Restart);

        LoadOptions();

        Vector2 newSpawn = Vector2.zero;
        CurrentPiece = Random.Range(0, 7);
        UsedPieces.Add(CurrentPiece);
        Rotation = 0;

        if(CurrentPiece == 0) {
            newSpawn = new Vector2(0,12);
        } else {
            newSpawn = new Vector2(-0.5f, 11.5f);
        }
        transform.position = newSpawn;

        CheckNextPiece();
        stop = true;
        starting = true;
        startTick = 60;
    }

    void Update()
    {
        Vector2 ghostPos = CheckFall();
        Vector2 nextPos;
        Vector2 holdPos;
        Color32 holdColor = new Color32();

        foreach(Transform Mino in transform) { //Remove old minos before drawing the piece again
            GameObject.Destroy(Mino.gameObject);
        }

        if(NextPiece == 0) {
            nextPos = new Vector2(8.5f,8f);
        } else if(NextPiece == 3) {
            nextPos = new Vector2(8f,8f);
        } else {
            nextPos = new Vector2(8.5f,8f);
        }
        if(HoldPiece == 0) {
            holdPos = new Vector2(-8.5f,8f);
        } else if(HoldPiece == 3) {
            holdPos = new Vector2(-9f,8f);
        } else {
            holdPos = new Vector2(-8.5f,8f);
        }
        if(hasHolded) {
            holdColor = Color.white;
        } else if(HoldPiece != 7) {
            holdColor = pieceColors[HoldPiece];
        }

        DrawPiece(NextPiece, 0, "Next", MinoSprite, nextPos, pieceColors[NextPiece], false, 5);
        if(HoldPiece != 7) {
            DrawPiece(HoldPiece, 0, "Hold", MinoSprite, holdPos, holdColor, false, 5);
        }
        DrawPiece(CurrentPiece, Rotation, "Mino", MinoSprite, transform.position, pieceColors[CurrentPiece], false, 5);
        DrawPiece(CurrentPiece, Rotation, "Ghost", GhostSprite, ghostPos, Color.white, false, 0);
    }

    void FixedUpdate()
    {
        if(!stop) {
            if(fallTime > GameSpeed && !isGrace) { //Attempt to move the piece down after time equal to game speed
                ApplyGravity();
            }
            if(fallTime > GameSpeed/SDF && isHD && !isGrace) { //Attempt to move the piece down after time equal to gamespeed/SDF
                bool didFall = ApplyGravity();
                if(didFall) {
                    TargetEmitter.PlayOneShot(SoftDropAudio);
                }
            }
            if(!isGrace) { //Count up the fall time
                fallTime++;
            } else {
                Vector2 checkPos = transform.position;
                checkPos.y--;
                bool isFloating = CheckPiece(checkPos, Rotation);

                if(isFloating) {
                    isGrace = false;
                } else if(grace < 0) {
                    LockPiece();
                } else if(isHD) {
                    grace -= 2;
                } else {grace--;}
            }

            if(currentMove != 0) {
                if(isAr || DAS < ARR) { //If DAS is less than ARR or we're already repeating, use ARR time
                    if(arTime > ARR) {
                        Vector2 moveApply = new Vector2(currentMove, 0);
                        bool isValid = ApplyMove(moveApply);
                        arTime = 0;

                        if(!isValid) {
                            isAr = false;
                        } else {
                            TargetEmitter.PlayOneShot(MoveAudio);
                        }
                    } else arTime++;
                } else if(arTime > DAS) { //Auto Repeat based on DAS if not already repeating
                    Vector2 moveApply = new Vector2(currentMove, 0);
                    bool isValid = ApplyMove(moveApply);
                    arTime = 0;

                    if(isValid) {
                        TargetEmitter.PlayOneShot(MoveAudio);
                        isAr = true;
                    }
                } else arTime++; //Increase the repeat counter if holding a movement key
            } else {
                arTime = 0;
                isAr = false;
            }

            if(dropDelay > 0) { //Decrease the drop delay if active
                dropDelay--;
            }

            if(moveBuffer > 0) { //Try to apply a move again if the buffer is active
                bool isValid = ApplyMove(new Vector2(moveBufferDir, 0));
                if(isValid) {
                    moveBuffer = 0;
                    TargetEmitter.PlayOneShot(MoveAudio);
                } else {moveBuffer--;}
            }
            if(rotateBuffer > 0) { //Try to apply a rotation again if the buffer is active
                int testRotation = Rotation;
                if(rotateBufferDir > 0) {
                    testRotation++;
                    if(testRotation > 3) {
                        testRotation = 0;
                    }
                }
                if(rotateBufferDir < 0) {
                    testRotation--;
                    if(testRotation < 0) {
                        testRotation = 3;
                    }
                }

                if(CurrentPiece != 3) {
                    Vector2 currentPos = transform.position;
                    Vector2 newPos = KickTest(transform.position, testRotation, rotateBufferDir);
                    bool isValid = newPos.y > -1000; //This is a scuffed but also kinda funny way to check if the rotation is valid

                    if(isValid) {
                        rotateBuffer = 0;
                        Rotation = testRotation;
                        ApplyMove(newPos - currentPos);
                        lastRotated = true;
                        TargetEmitter.PlayOneShot(RotateAudio);
                    }
                    rotateBuffer--;
                } else {rotateBuffer = 0;}
            }
        }

        if(starting && !paused) {
            Text countText = startText.GetComponent<Text>();
            Color32 opacity = new Color32();
            RectTransform countTransform = startText.GetComponent<RectTransform>();
            Vector2 countScale = countTransform.localScale;

            opacity = countText.color;

            if(startTick == 0 && startStage < 5) {
                countScale = new Vector2(13, 13);
                startStage++;
                startTick = 60;

                if(startStage < 4) {
                    TargetEmitter.PlayOneShot(StartAudio);
                }
                if(startStage == 4) {
                    TargetEmitter.PlayOneShot(Start1Audio);
                    TargetEmitter.PlayOneShot(Start2Audio);
                }
            } else {
                startTick--;
            }

            if(startStage == 0) {
                startText.SetActive(false);
                countScale = new Vector2(13, 13);
                opacity.a = 255;
                countText.color = opacity;
            }
            else if(startStage == 1) {
                startText.SetActive(true);
                countText.text = "3";
                countScale.x -= 0.05f;
                countScale.y -= 0.05f;
            }
            else if(startStage == 2) {
                countText.text = "2";
                countScale.x -= 0.05f;
                countScale.y -= 0.05f;
            }
            else if(startStage == 3) {
                countText.text = "1";
                countScale.x -= 0.05f;
                countScale.y -= 0.05f;
            }
            else if(startStage == 4) {
                countText.text = "GO!";
                countScale.x -= 0.05f;
                countScale.y -= 0.05f;
                if(opacity.a > 4) {
                    opacity.a -= 3;
                } else opacity.a = 0;
                countText.color = opacity;
            } else {
                startText.SetActive(false);
                starting = false;
                StartGame();
            }
            
            countTransform.localScale = countScale;
        }
    }
}