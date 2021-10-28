using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FlairControl : MonoBehaviour
{
    [SerializeField] private ClearLines ClearLines;

    [SerializeField] private GameObject LargeCenter;
    [SerializeField] private GameObject MedSide;
    [SerializeField] private GameObject SmallSide;
    [SerializeField] private GameObject SmallScore;
    [SerializeField] private Color32 TspinColor;
    [SerializeField] private Color32 MiniColor;
    [SerializeField] private Color32 QuadColor;
    [SerializeField] private Color32 LineCountColor;

    private int LargeCenterLife;
    private int MedSideLife;
    private int SmallSideLife;
    private int SmallScoreLife;
    private bool DoLargeCenter;
    private bool DoMedSide;
    private bool DoSmallSide;
    private bool DoSmallScore;
    private int LineCount;
    private int SpinType;
    private byte alpha = (byte)255;
    private int oldB2B = 0;
    private int oldCombo = 0;
    private int comboScore;

    string DrawScoreText(byte alpha)
    {
        Color32 currentSpinColor = TspinColor;
        Color32 currentMiniColor = MiniColor;
        Color32 currentQuadColor = QuadColor;
        Color32 currentCountColor = LineCountColor;

        currentSpinColor.a = alpha;
        currentMiniColor.a = alpha;
        currentQuadColor.a = alpha;
        currentCountColor.a = alpha;

        string miniText = "";
        string spinText = "";
        string countText = "";
        if(SpinType > 0) {
            spinText = "<color=#" + ColorUtility.ToHtmlStringRGBA(currentSpinColor) + ">T-SPIN</color>";
            if(SpinType == 1) {
                miniText = "<color=#" + ColorUtility.ToHtmlStringRGBA(currentMiniColor) + ">mini</color>";
            }
        }
        if(LineCount == 4) {
            countText = "<color=#" + ColorUtility.ToHtmlStringRGBA(currentQuadColor) + ">QUAD</color>";
        } else if(LineCount == 3) {
            countText = "<color=#" + ColorUtility.ToHtmlStringRGBA(currentCountColor) + "> Triple</color>";
        } else if(LineCount == 2) {
            countText = "<color=#" + ColorUtility.ToHtmlStringRGBA(currentCountColor) + "> Double</color>";
        } else if(LineCount == 1) {
            countText = "<color=#" + ColorUtility.ToHtmlStringRGBA(currentCountColor) + "> Single</color>";
        }
        return(miniText + spinText + countText);
    }

    void UpdateFlair(object sender, ClearLines.PointsEarnedArgs args)
    {
        if(args.LineCount > 0 || args.SpinType > 0) {
            Text clearText = MedSide.GetComponent<Text>();
            RectTransform scoreTransform = MedSide.GetComponent<RectTransform>();

            LineCount = args.LineCount;
            SpinType = args.SpinType;

            clearText.text = DrawScoreText(0);
            alpha = 0;
            scoreTransform.localScale = new Vector2(7f, 7f);
            scoreTransform.localPosition = new Vector2(-473f, 0f);

            MedSide.SetActive(true);
            MedSideLife = 90;
            DoMedSide = true;
        }

        string displayB2B = "";
        string displayCombo = "";

        if(args.B2BLength > 1) {
            string thisB2B = args.B2BLength.ToString();
            displayB2B = "B2B " + thisB2B;
        }
        if(args.ComboLength > 0) {
            string thisCombo = args.ComboLength.ToString();
            displayCombo = " Combo " + thisCombo;
        }

        Text comboText = SmallSide.GetComponent<Text>();
        RectTransform comboTransform = SmallSide.GetComponent<RectTransform>();
        if((args.B2BLength > oldB2B && args.B2BLength > 1) || args.ComboLength > oldCombo) {
            comboTransform.localScale = new Vector2(4.5f, 4.5f);
            comboTransform.localPosition = new Vector2(-365.5f, -59f);

            SmallSide.SetActive(true);

            DoSmallSide = true;
            SmallSideLife = 40;
        } else if(args.B2BLength < 2 && args.ComboLength == 0) {
            if(oldB2B > 1) displayB2B = "B2B " + oldB2B;
            if(oldCombo > 0) displayCombo = " Combo " + oldCombo;

            DoSmallSide = false;
        }

        comboText.text = displayB2B + displayCombo;
        oldB2B = args.B2BLength;
        oldCombo = args.ComboLength;

        if(args.ScoreGain > 0) {
            Text scoreText = SmallScore.GetComponent<Text>();
            RectTransform scoreTransform = SmallScore.GetComponent<RectTransform>();
            Color32 currentColor = scoreText.color;

            comboScore += args.ScoreGain;
            string toDisplay = comboScore.ToString();

            currentColor.a = 0;
            scoreText.color = currentColor;

            SmallScore.SetActive(true);
            scoreTransform.localPosition = new Vector2(460f, -25f);

            scoreText.text = "+" + toDisplay;
            DoSmallScore = true;
            SmallScoreLife = 30;
        } else {
            comboScore = 0;
            DoSmallScore = false;
        }

        if(args.AllClear) {
            Text centerText = LargeCenter.GetComponent<Text>();

            LargeCenter.SetActive(true);
            LargeCenter.transform.localScale = new Vector2(25f, 25f);

            centerText.text = "ALL CLEAR";
            LargeCenterLife = 120;
            DoLargeCenter = true;
        }
    }

    void Start()
    {
        ClearLines.OnPointsEarned += UpdateFlair;
    }

    void FixedUpdate()
    {
        if(DoMedSide == true) {
            RectTransform scoreTransform = MedSide.GetComponent<RectTransform>();
            Vector2 clearScale = scoreTransform.localScale;
            Vector2 clearPos = scoreTransform.localPosition;
            Text clearText = MedSide.GetComponent<Text>();

            if(MedSideLife > 80) {
                clearScale.x -= 0.1f;
                clearScale.y -= 0.1f;
                clearPos.x += 1f;
                if(alpha < 225) {
                    alpha += 24;
                } else alpha = 255;
            } else if(MedSideLife > 20) {
                alpha = 255;
            } else if(MedSideLife > 0) {
                if(alpha > 12) {
                    alpha -= 12;
                } else alpha = 0;
            } else if(MedSideLife == 0) {
                MedSide.SetActive(false);
                DoMedSide = false;
            }
            clearPos.x -= 0.5f;

            clearText.text = DrawScoreText(alpha);

            scoreTransform.localScale = clearScale;
            scoreTransform.localPosition = clearPos;

            MedSideLife--;
        } else MedSide.SetActive(false);

        if(SmallSideLife > 0) {
            RectTransform comboTransform = SmallSide.GetComponent<RectTransform>();
            Vector2 comboPos = comboTransform.localPosition;
            Vector2 comboScale = comboTransform.localScale;
            Text comboText = SmallSide.GetComponent<Text>();
            Color32 currentColor = comboText.color;

            if(DoSmallSide) {
                currentColor.a = 255;
                if(comboScale.x > 3.75f) {
                    comboScale.x -= 0.25f;
                    comboScale.y -= 0.25f;
                    comboPos.x += 11f;
                }
            } else {
                if(currentColor.a > 6) {
                    currentColor.a -= 6;
                } else currentColor.a = 0;

                SmallSideLife--;
            }
            comboTransform.localPosition = comboPos;
            comboTransform.localScale = comboScale;
            comboText.color = currentColor;
        } else SmallSide.SetActive(false);

        if(SmallScoreLife > 0) {
            RectTransform scoreTransform = SmallScore.GetComponent<RectTransform>();
            Vector2 scorePos = scoreTransform.localPosition;
            Text scoreText = SmallScore.GetComponent<Text>();
            Color32 currentColor = scoreText.color;

            if(DoSmallScore) {
                if(scorePos.y > -39f) {
                    scorePos.y -= 1f;
                }
                if(currentColor.a < 237) {
                    currentColor.a += 18;
                } else {currentColor.a = 255;}
            } else {
                scorePos.y = -39f;
                if(currentColor.a > 8) {
                    currentColor.a -= 8;
                } else currentColor.a = 0;

                SmallScoreLife--;
            }
            scoreText.color = currentColor;
            scoreTransform.localPosition = scorePos;
        } else SmallScore.SetActive(false);

        if(DoLargeCenter) {
            Text centerText = LargeCenter.GetComponent<Text>();
            Vector2 currentScale = LargeCenter.transform.localScale;
            Color32 currentColor = centerText.color;

            if(LargeCenterLife == 120) {
                currentColor.a = 0;
            } else if(LargeCenterLife > 110) {
                currentScale.x -= 1.5f;
                currentScale.y -= 1.5f;
                currentColor.a += 25;
            } else if(LargeCenterLife > 90) {
                currentScale.x -= 0.02f;
                currentScale.y -= 0.02f;
            } else if(LargeCenterLife > 0) {
                currentScale.x -= 0.05f;
                currentScale.y -= 0.05f;
                if(currentColor.a > 3) {
                    currentColor.a -= 3;
                } else currentColor.a = 0;
            } else {
                LargeCenter.SetActive(false);
                DoLargeCenter = false;
            }

            if(currentColor.a > 250) currentColor.a = 255;
            LargeCenter.transform.localScale = currentScale;
            centerText.color = currentColor;
            
            LargeCenterLife--;
        }
    }
}
