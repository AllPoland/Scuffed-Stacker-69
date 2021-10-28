using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearLines : MonoBehaviour
{
    [SerializeField] private LayerMask CollisionMask;
    [SerializeField] private GameObject PlayerController;

    [SerializeField] private AudioSource TargetEmitter;
    [SerializeField] private AudioClip LineClearSound;
    [SerializeField] private AudioClip HardClearSound;
    [SerializeField] private AudioClip AllClearSound;
    [SerializeField] private AudioClip BigPointSound;
    [SerializeField] private string ComboClipName;

    public List<AudioClip> comboClips = new List<AudioClip>();

    public event EventHandler<PointsEarnedArgs> OnPointsEarned;
    public class PointsEarnedArgs : EventArgs
    {
        public int LineCount;
        public int ScoreGain;
        public int SpinType;
        public int ComboLength;
        public int B2BLength;
        public bool AllClear;
    }

    private int B2BToSend = 0;

    private void Start()
    {
        for(int n = 0; n < 20; n++) //Load the 20 combo sound effects and add them to the list
        {
            string addNumber = n.ToString();
            AudioClip toAdd = Resources.Load("Sounds/SoundEffects/" + ComboClipName + addNumber, typeof(AudioClip)) as AudioClip;
            comboClips.Add(toAdd);
        }
    }

    private void Delete(GameObject target) //Delete the target object
    {
        GameObject.Destroy(target);
    }

    private void PushDown(int pushAmount, float minHeight) //Push down all minos above a certain height
    {
        GameObject[] moveTargets = GameObject.FindGameObjectsWithTag("PlacedMino");
        int iterations = 0;
        foreach(GameObject Mino in moveTargets)
        {
            Vector2 targetPos = Mino.transform.position;
            if(targetPos.y > minHeight) {
                targetPos.y += pushAmount;
                Mino.transform.position = targetPos;
                iterations++;
            }
        }
    }

    private bool AllClear()
    {
        for(int n = 0; n < 21; n++)
        {
            GameObject lineCheck = GameObject.Find("Line" + n);
            Vector2 checkPos = lineCheck.transform.position;

            bool hasMino = Physics2D.OverlapBox(checkPos, new Vector2(10f,0.5f), 0, CollisionMask);
            if(hasMino) {
                return(false);
            }
        }
        return(true);
    }

    public void ClearBoard()
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag("PlacedMino");

        for(int n = 0; n < targets.Length; n++)
        {
            Delete(targets[n]);
        }
    }

    public int LineClear(int usedPiece, int pieceRotation, Vector2 piecePos, int isSpin) //Clears any full lines, moves top lines down, and returns the total score gained
    {
        PieceControl playerScript = PlayerController.GetComponent<PieceControl>();
        int clearedLines = 0;
        int gainedScore = 0;
        int line = 0;
        int Combo = playerScript.Combo;
        int B2B = playerScript.B2B;
        bool isB2B = false;
        bool cleared = false;

        while(cleared == false)
        {
            Collider2D[] detectedMinos;
            GameObject lineCheck = GameObject.Find("Line" + line);
            Vector2 checkPos = lineCheck.transform.position;

            detectedMinos = Physics2D.OverlapBoxAll(checkPos, new Vector2(10f,0.5f), 0, CollisionMask); //Get a list of every mino in the line
            int lineFill = detectedMinos.Length;

            if(lineFill > 9) { //If the line has 10 collided minos, push higher rows down and delete this row
                clearedLines++;
                
                PushDown(-1, detectedMinos[0].transform.position.y);
                foreach(Collider2D Mino in detectedMinos)
                {
                    Delete(Mino.gameObject);
                }

                line = 0;
            } else {
                line++;
            }
            if(line > 20) {
                cleared = true;
            }
        }

        if(clearedLines == 4) {
            isB2B = true;
            gainedScore = 4;
        } else if(clearedLines > 0) {
            gainedScore = clearedLines - 1;
        }

        if(isSpin == 1) {
            gainedScore = clearedLines;
        }
        if(isSpin == 2) {
            if(clearedLines > 0) {
                isB2B = true;
            }
            
            gainedScore = clearedLines*2;
        }

        bool allClear = AllClear();
        int comboToSend = 0;
        if(clearedLines > 0) {
            if(allClear) {
                gainedScore += 9;
                isB2B = true;
                TargetEmitter.PlayOneShot(AllClearSound);
            }

            if(Combo < 4) {
                gainedScore += Combo;
            } else {gainedScore += 4;}

            if(Combo > 0) {
                comboToSend = Combo;

                if(Combo <= comboClips.Count) {
                    TargetEmitter.PlayOneShot(comboClips[Combo-1]);
                } else {TargetEmitter.PlayOneShot(comboClips[18]);}
            }
            playerScript.Combo++;

            if(!isB2B) {
                playerScript.B2B = 0;
                B2BToSend = 0;
                if(B2B > 2) {
                    Debug.Log("B2B streak ended");
                }
            } else if(B2B > 1) {
                gainedScore += 1;
                if(B2B > 3) {
                    gainedScore += 1;
                }
            }
            if(isB2B) {
                B2BToSend = B2B;
                playerScript.B2B++;
                TargetEmitter.PlayOneShot(HardClearSound);
            }

            TargetEmitter.PlayOneShot(LineClearSound);
        } else {playerScript.Combo = 0;}
        
        PointsEarnedArgs newArgs = new PointsEarnedArgs
        {
            LineCount = clearedLines,
            ScoreGain = gainedScore,
            SpinType = isSpin,
            ComboLength = comboToSend,
            B2BLength = B2BToSend,
            AllClear = allClear
        };
        OnPointsEarned?.Invoke(this, newArgs);

        if(gainedScore > 6) {
            TargetEmitter.PlayOneShot(BigPointSound);
        }

        playerScript.totalLines += clearedLines;
        playerScript.Score += gainedScore;
        return(gainedScore);
    }
}