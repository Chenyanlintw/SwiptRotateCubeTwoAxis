using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameControl : MonoBehaviour
{
    // 方塊物件
    public GameObject Cube;

    // 目標旋轉角度
    private float desireAngle = 0;

    // 保存觸控前的角度（計算用）
    private float prevAngle = 0;
    
    // 紀錄觸控按下時的座標
    private float touchStartX = 0;
    private float touchStartY = 0;

    // 將要旋轉的軸向
    private Vector3 activeAxis;

    // 是否旋轉完成
    private bool isRotateFinish = true;

    // 旋轉時動態產生的父物件，包入方塊物件，再旋轉此物件（不直接轉方塊）
    // 當觸控到與上次左右不同邊時，就重新產生
    //（因此解決軸向判斷、萬象鎖的複雜問題）
    private GameObject cubeRotator;

    // 紀錄上次螢幕觸碰左右邊 (LEFT/RIGHT)
    string prevTouchSide = "";


    void Start()
    {
        
    }


    void Update()
    {
        handleCubeTouchRotate();
    }


    // 處理方塊觸控旋轉
    void handleCubeTouchRotate()
    {
        // 有觸控發生
        // 觸控部分主要用來：
        // 產生旋轉物件(cubeRotator)、取得旋轉軸向(activeAxis)、目標角度(desireAngle)
        if (Input.touchCount > 0)
        {
            // --------------------------------------------
            // 觸碰當下（執行一次）
            if (Input.touches[0].phase == TouchPhase.Began)
            {
                // 記錄開始觸碰時的資訊
                touchStartX = Input.touches[0].position.x;
                touchStartY = Input.touches[0].position.y;

                // 判斷觸控在螢幕右邊或左邊
                string currTouchSide = touchStartX <= Screen.width / 2 ? "LEFT" : "RIGHT";

                // 依螢幕部分設定將來旋轉軸向
                if (currTouchSide == "LEFT")
                    activeAxis = new Vector3(1, 0, 0); // X軸

                if (currTouchSide == "RIGHT")
                    activeAxis = new Vector3(0, 0, 1); // Z軸

                // 如果觸碰螢幕部分和上次不同
                // 要重新產生用來旋轉方塊的父物件
                if (currTouchSide != prevTouchSide)
                {
                    // 如果之前已有父物件，則清除
                    if (cubeRotator)
                    {
                        Destroy(cubeRotator);
                        Cube.transform.SetParent(null);
                    }

                    // 產生新的父物件
                    cubeRotator = new GameObject("Rotater");
                    Cube.transform.SetParent(cubeRotator.transform, false);

                    // 重設旋轉
                    prevAngle = 0;
                    desireAngle = 0;

                    // 紀錄目前碰觸螢幕區塊，供下次判斷
                    prevTouchSide = currTouchSide;
                }

                // 開始旋轉
                isRotateFinish = false;
            }

            // --------------------------------------------
            // 觸碰移動（持續）
            if (Input.touches[0].phase == TouchPhase.Moved)
            {
                // 計算手指垂直拖曳距離
                Vector2 touchPoint = Input.touches[0].position;
                float delta = touchPoint.y - touchStartY;

                // 將距離轉換為目標旋轉角度 （觸控前角度＋拖曳距離/減小係數） 
                desireAngle = prevAngle + delta / 5.0f;
            }

            // --------------------------------------------
            // 觸碰放開 (執行一次)
            if (Input.touches[0].phase == TouchPhase.Ended)
            {
                // 將目標角度轉為 90 的倍數
                desireAngle = Mathf.Round(desireAngle / 90f) * 90f;

                // 紀錄最後目標角度（下次觸控前的角度）
                prevAngle = desireAngle;
            }
        }


        // 實際方塊旋轉
        // 利用上方觸控得到的 旋轉軸向(activeAxis)、目標角度(desireAngle)
        // 去旋轉方塊
        if (!isRotateFinish && cubeRotator)
        {
            // 目標角度轉換為旋轉量（四元數）
            Quaternion desireRotation = Quaternion.Euler(activeAxis * desireAngle);

            // 計算剩餘旋轉角度(sin)(數值越大，角度越小）
            float deltaAngle = Mathf.Abs(Quaternion.Dot(cubeRotator.transform.rotation, desireRotation));

            // 如果剩餘角度多於約１度
            if (deltaAngle < 0.999f)
            {
                // 讓方塊持續轉到目標角度(Lerp)
                cubeRotator.transform.rotation = Quaternion.Lerp(cubeRotator.transform.rotation, desireRotation, 0.5f);
            }

            // 如果剩餘角度少於約１度（Lerp旋轉到接近的目標度數了）
            if (deltaAngle >= 0.999f)
            {
                // 且是在已放開觸控時
                if (Input.touchCount == 0)
                {
                    // 直接將角度轉為 90 度的倍數（讓數值乾淨、旋轉到正位）
                    cubeRotator.transform.rotation = changeEulerToIsoAngles(cubeRotator.transform.rotation);

                    // 停止自動旋轉漸變(lerp)
                    isRotateFinish = true;

                    // 旋轉結束函式
                    onCubeRotateCompleted();
                }
            }
        }
    }

    // 當每次旋轉結束
    void onCubeRotateCompleted()
    {
        // 在旋轉到正位後要做的事，可加在這裡
    }

    // 將旋轉都四捨五入為 90 度的倍數 
    private Quaternion changeEulerToIsoAngles(Quaternion rotation)
    {
        Vector3 euler = rotation.eulerAngles;

        return Quaternion.Euler(
            Mathf.Round(euler.x / 90f) * 90f,
            Mathf.Round(euler.y / 90f) * 90f,
            Mathf.Round(euler.z / 90f) * 90f);
    }
}
